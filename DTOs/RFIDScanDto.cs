using System;
using System.ComponentModel.DataAnnotations;

namespace RFIDAttendanceAPI.Models
{
    public class RFIDScanDto
    {
        [Required]
        [StringLength(50)]
        public string UID { get; set; } = string.Empty;

        public DateTime? ScanTime { get; set; }

        public bool IsManual { get; set; } = false;
    }

    public class RFIDScanResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string RFID_UID { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public AttendanceRecordDto? Attendance { get; set; }
    }

    public class AttendanceRecordDto
    {
        public int AttendanceID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string RFID_UID { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string? TimeIn { get; set; }
        public string? TimeOut { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}