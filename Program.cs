using Microsoft.EntityFrameworkCore;
using RFIDAttendanceAPI.Data;
using RFIDAttendanceAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5190);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RFID Attendance API V1");
        c.RoutePrefix = string.Empty;
    });
}

//app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Test database connection on startup
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
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database connection error");
    }
}

app.Run();