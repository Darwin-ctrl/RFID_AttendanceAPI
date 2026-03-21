using Microsoft.EntityFrameworkCore;
using RFIDAttendanceAPI.Data;
using RFIDAttendanceAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Kunin ang PORT mula sa environment variable ng Render (default: 8080)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// 2. Alisin ang fixed port binding (5190) para iwas conflict
//    HUWAG na gamitin ang:
//    builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(5190); });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Database connection – siguraduhing tama ang provider
//    Kung PostgreSQL ang gamit mo (tulad ng sa Render), gamitin ang Npgsql.
//    I-install ang package: Npgsql.EntityFrameworkCore.PostgreSQL
//    Kung SQL Server talaga, iwan ang UseSqlServer.
//    I-adjust ang connection string name kung iba ang environment variable.

// Halimbawa para sa PostgreSQL (RECOMMENDED para sa Render):
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(connectionString));

// Kung SQL Server ang gamit, palitan ng:
// builder.Services.AddDbContext<DatabaseContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IRFIDService, RFIDService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

// Swagger configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RFID Attendance API V1");
        c.RoutePrefix = string.Empty;
    });
}

// Para sa production, pwede mo ring i-enable ang Swagger kung gusto
// app.UseSwagger();
// app.UseSwaggerUI();

// app.UseHttpsRedirection(); // I-disable muna kung walang HTTPS setup
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Test database connection on startup (hindi na kailangang mag-exit kung failed)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Testing database connection...");
        var canConnect = dbContext.Database.CanConnect();
        logger.LogInformation("Can connect to database: {CanConnect}", canConnect);

        if (canConnect)
        {
            var studentCount = dbContext.Students.Count();
            var attendanceCount = dbContext.Attendances.Count();
            logger.LogInformation("Students: {StudentCount}, Attendance: {AttendanceCount}",
                studentCount, attendanceCount);
        }
        else
        {
            logger.LogWarning("Cannot connect to database. The app will continue but some features may fail.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database connection error. The app will continue but some features may fail.");
    }
}

app.Run();