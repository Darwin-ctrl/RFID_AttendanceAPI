using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RFIDAttendanceAPI.Models
{
    [Table("Attendance")]
    public class Attendance
    {
        [Key]
        public int AttendanceID { get; set; }

        [Required]
        [StringLength(50)]
        public string RFID_UID { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public DateTime DateToday { get; set; }

        public DateTime? TimeIn { get; set; }

        public DateTime? TimeOut { get; set; }

        // REMOVE these lines since they don't exist in your database:
        // public int? StudentID { get; set; }
        // [ForeignKey("StudentID")]
        // public Student? Student { get; set; }
    }
}