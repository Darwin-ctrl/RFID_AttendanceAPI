using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RFIDAttendanceAPI.Data;
using RFIDAttendanceAPI.Models;

namespace RFIDAttendanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(DatabaseContext context, ILogger<AttendanceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("test")]
        public async Task<ActionResult> TestConnection()
        {
            try
            {
                var result = new
                {
                    timestamp = DateTime.Now,
                    database = new
                    {
                        canConnect = await _context.Database.CanConnectAsync(),
                        connectionString = _context.Database.GetConnectionString(),
                        provider = _context.Database.ProviderName
                    },
                    tables = new
                    {
                        studentsExist = await _context.Students.AnyAsync(),
                        attendanceExist = await _context.Attendances.AnyAsync(),
                        studentCount = await _context.Students.CountAsync(),
                        attendanceCount = await _context.Attendances.CountAsync()
                    },
                    today = DateTime.Today
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error testing connection",
                    error = ex.Message
                });
            }
        }

        [HttpGet("students")]
        public async Task<ActionResult> GetAllStudents()
        {
            try
            {
                var students = await _context.Students
                    .Select(s => new
                    {
                        s.StudentID,
                        s.RFID_UID,
                        s.FullName,
                        s.ParentPhone
                    })
                    .OrderBy(s => s.FullName)
                    .ToListAsync();

                return Ok(new
                {
                    count = students.Count,
                    students = students
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("check-today")]
        public async Task<ActionResult> CheckTodayAttendance()
        {
            try
            {
                DateTime today = DateTime.Today;

                var attendanceForToday = await _context.Attendances
                    .Where(a => a.DateToday == today)
                    .OrderByDescending(a => a.TimeIn)
                    .Select(a => new
                    {
                        a.AttendanceID,
                        a.RFID_UID,
                        a.FullName,
                        TimeIn = a.TimeIn != null ? a.TimeIn.Value.ToString("hh:mm:ss tt") : null,
                        TimeOut = a.TimeOut != null ? a.TimeOut.Value.ToString("hh:mm:ss tt") : null,
                        Status = a.TimeOut == null ? "Time In Only" : "Completed"
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Date = today.ToString("MMMM dd, yyyy"),
                    HasAttendance = attendanceForToday.Any(),
                    Count = attendanceForToday.Count,
                    Records = attendanceForToday
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("today")]
        public async Task<ActionResult<IEnumerable<AttendanceRecordDto>>> GetTodayAttendance(
            [FromQuery] string search = "")
        {
            try
            {
                DateTime today = DateTime.Today;
                var query = _context.Attendances
                    .Where(a => a.DateToday == today)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(a =>
                        a.RFID_UID.Contains(search) ||
                        a.FullName.Contains(search));
                }

                var attendances = await query
                    .OrderByDescending(a => a.TimeIn)
                    .Select(a => new AttendanceRecordDto
                    {
                        AttendanceID = a.AttendanceID,
                        StudentName = a.FullName,
                        RFID_UID = a.RFID_UID,
                        Date = a.DateToday.ToString("MMMM dd, yyyy"),
                        TimeIn = a.TimeIn != null ? a.TimeIn.Value.ToString("hh:mm:ss tt") : null,
                        TimeOut = a.TimeOut != null ? a.TimeOut.Value.ToString("hh:mm:ss tt") : null,
                        Status = a.TimeOut == null ? "Time In Only" : "Completed"
                    })
                    .ToListAsync();

                return Ok(attendances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today's attendance");
                return StatusCode(500, new { message = "Error retrieving attendance data" });
            }
        }

        [HttpGet("student/{uid}")]
        public async Task<ActionResult<IEnumerable<AttendanceRecordDto>>> GetStudentAttendance(
            string uid,
            [FromQuery] int days = 30)
        {
            try
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.RFID_UID == uid);

                var startDate = DateTime.Today.AddDays(-days);

                var attendances = await _context.Attendances
                    .Where(a => a.RFID_UID == uid && a.DateToday >= startDate)
                    .OrderByDescending(a => a.DateToday)
                    .ThenByDescending(a => a.TimeIn)
                    .Select(a => new AttendanceRecordDto
                    {
                        AttendanceID = a.AttendanceID,
                        StudentName = a.FullName,
                        RFID_UID = a.RFID_UID,
                        Date = a.DateToday.ToString("MMMM dd, yyyy"),
                        TimeIn = a.TimeIn != null ? a.TimeIn.Value.ToString("hh:mm:ss tt") : null,
                        TimeOut = a.TimeOut != null ? a.TimeOut.Value.ToString("hh:mm:ss tt") : null,
                        Status = a.TimeOut == null ? "Time In Only" : "Completed"
                    })
                    .ToListAsync();

                return Ok(new
                {
                    StudentName = student?.FullName ?? "Unknown",
                    StudentExists = student != null,
                    DaysRequested = days,
                    StartDate = startDate.ToString("MMMM dd, yyyy"),
                    TotalRecords = attendances.Count,
                    Records = attendances
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student attendance for UID: {UID}", uid);
                return StatusCode(500, new { message = "Error retrieving attendance data" });
            }
        }

        [HttpGet("stats/today")]
        public async Task<ActionResult> GetTodayStats()
        {
            try
            {
                DateTime today = DateTime.Today;

                var totalScans = await _context.Attendances
                    .CountAsync(a => a.DateToday == today);

                var timeInOnly = await _context.Attendances
                    .CountAsync(a => a.DateToday == today && a.TimeOut == null);

                var completed = await _context.Attendances
                    .CountAsync(a => a.DateToday == today && a.TimeOut != null);

                var uniqueStudents = await _context.Attendances
                    .Where(a => a.DateToday == today)
                    .Select(a => a.RFID_UID)
                    .Distinct()
                    .CountAsync();

                var totalStudents = await _context.Students.CountAsync();

                return Ok(new
                {
                    Date = today.ToString("MMMM dd, yyyy"),
                    TotalScans = totalScans,
                    UniqueStudents = uniqueStudents,
                    TimeInOnly = timeInOnly,
                    Completed = completed,
                    TotalStudents = totalStudents,
                    HasAttendance = totalScans > 0,
                    Message = totalScans == 0 ? "No attendance records for today" : "Attendance records found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today's stats");
                return StatusCode(500, new
                {
                    message = "Error retrieving statistics",
                    error = ex.Message
                });
            }
        }
    }
}