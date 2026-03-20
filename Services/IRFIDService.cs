using RFIDAttendanceAPI.Models;

namespace RFIDAttendanceAPI.Services
{
    public interface IRFIDService
    {
        Task<RFIDScanResponseDto> ProcessRFIDScan(string uid, bool isManual = false);
        Task<Student?> GetStudentByUID(string uid);
        Task<Attendance?> GetLatestAttendanceToday(string uid);
        Task<Attendance> CreateTimeIn(string uid, string fullname);
        Task<Attendance?> UpdateTimeOut(int attendanceId);
    }
}