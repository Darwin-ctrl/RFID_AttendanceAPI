using Microsoft.EntityFrameworkCore;
using RFIDAttendanceAPI.Data;
using RFIDAttendanceAPI.Models;

namespace RFIDAttendanceAPI.Services
{
    public class RFIDService : IRFIDService
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<RFIDService> _logger;

        public RFIDService(DatabaseContext context, ILogger<RFIDService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RFIDScanResponseDto> ProcessRFIDScan(string uid, bool isManual = false)
        {
            var response = new RFIDScanResponseDto
            {
                Success = false,
                RFID_UID = uid,
                Timestamp = DateTime.Now
            };

            try
            {
                // Check if student exists - FIXED: removed IsActive filter
                var student = await GetStudentByUID(uid);
                if (student == null)
                {
                    response.Message = "RFID NOT REGISTERED!";
                    return response;
                }

                response.StudentName = student.FullName;

                DateTime today = DateTime.Today;

                var latestAttendance = await _context.Attendances
                    .Where(a => a.RFID_UID == uid && a.DateToday == today)
                    .OrderByDescending(a => a.AttendanceID)
                    .FirstOrDefaultAsync();

                Attendance attendance;

                if (latestAttendance == null)
                {
                    attendance = await CreateTimeIn(uid, student.FullName);
                    response.Action = "TIME_IN";
                    response.Message = "TIME IN RECORDED";
                    response.Success = true;
                }
                else if (latestAttendance.TimeOut == null)
                {
                    attendance = await UpdateTimeOut(latestAttendance.AttendanceID);
                    response.Action = "TIME_OUT";
                    response.Message = "TIME OUT RECORDED";
                    response.Success = true;
                }
                else
                {
                    attendance = await CreateTimeIn(uid, student.FullName);
                    response.Action = "TIME_IN";
                    response.Message = "TIME IN RECORDED";
                    response.Success = true;
                }

                if (attendance != null)
                {
                    response.Attendance = new AttendanceRecordDto
                    {
                        AttendanceID = attendance.AttendanceID,
                        StudentName = student.FullName,
                        RFID_UID = attendance.RFID_UID,
                        Date = attendance.DateToday.ToString("MMMM dd, yyyy"),
                        TimeIn = attendance.TimeIn?.ToString("hh:mm:ss tt"),
                        TimeOut = attendance.TimeOut?.ToString("hh:mm:ss tt"),
                        Status = attendance.TimeOut == null ? "Time In Only" : "Completed"
                    };
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RFID scan for UID: {UID}", uid);
                response.Message = $"Error: {ex.Message}";
                return response;
            }
        }

        public async Task<Student?> GetStudentByUID(string uid)
        {
            // FIXED: removed .Where(s => s.IsActive)
            return await _context.Students
                .Where(s => s.RFID_UID == uid)
                .FirstOrDefaultAsync();
        }

        public async Task<Attendance?> GetLatestAttendanceToday(string uid)
        {
            DateTime today = DateTime.Today;
            return await _context.Attendances
                .Where(a => a.RFID_UID == uid && a.DateToday == today)
                .OrderByDescending(a => a.AttendanceID)
                .FirstOrDefaultAsync();
        }

        public async Task<Attendance> CreateTimeIn(string uid, string fullname)
        {
            var attendance = new Attendance
            {
                RFID_UID = uid,
                FullName = fullname,
                DateToday = DateTime.Today,
                TimeIn = DateTime.Now
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return attendance;
        }

        public async Task<Attendance?> UpdateTimeOut(int attendanceId)
        {
            var attendance = await _context.Attendances.FindAsync(attendanceId);
            if (attendance != null)
            {
                attendance.TimeOut = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return attendance;
        }
    }
}