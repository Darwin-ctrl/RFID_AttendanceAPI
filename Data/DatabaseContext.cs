using Microsoft.EntityFrameworkCore;
using RFIDAttendanceAPI.Models;

namespace RFIDAttendanceAPI.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Student configuration
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.StudentID);
                entity.HasIndex(e => e.RFID_UID).IsUnique();
            });

            // Attendance configuration - REMOVED the relationship with Student
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => e.AttendanceID);
                entity.HasIndex(e => new { e.RFID_UID, e.DateToday });

                // REMOVED: .HasOne(e => e.Student).WithMany(e => e.Attendances)...
            });

            // Seed test data (optional)
            modelBuilder.Entity<Student>().HasData(
                new Student
                {
                    StudentID = 1,
                    RFID_UID = "1234567890",
                    FullName = "John Doe",
                    ParentPhone = "09123456789"
                },
                new Student
                {
                    StudentID = 2,
                    RFID_UID = "0987654321",
                    FullName = "Jane Smith",
                    ParentPhone = "09987654321"
                }
            );
        }
    }
}