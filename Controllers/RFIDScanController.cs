using Microsoft.AspNetCore.Mvc;
using RFIDAttendanceAPI.Models;
using RFIDAttendanceAPI.Services;

namespace RFIDAttendanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RFIDScanController : ControllerBase
    {
        private readonly IRFIDService _rfidService;
        private readonly ILogger<RFIDScanController> _logger;

        public RFIDScanController(IRFIDService rfidService, ILogger<RFIDScanController> logger)
        {
            _rfidService = rfidService;
            _logger = logger;
        }

        // POST: api/RFIDScan/scan
        [HttpPost("scan")]
        public async Task<ActionResult<RFIDScanResponseDto>> ScanRFID([FromBody] RFIDScanDto scanDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _rfidService.ProcessRFIDScan(scanDto.UID, scanDto.IsManual);

            if (!result.Success && result.Message == "RFID NOT REGISTERED!")
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        // POST: api/RFIDScan/manual-scan
        [HttpPost("manual-scan")]
        public async Task<ActionResult<RFIDScanResponseDto>> ManualScan([FromBody] RFIDScanDto scanDto)
        {
            scanDto.IsManual = true;
            return await ScanRFID(scanDto);
        }

        // GET: api/RFIDScan/check-student/{uid}
        [HttpGet("check-student/{uid}")]
        public async Task<ActionResult<Student>> CheckStudent(string uid)
        {
            var student = await _rfidService.GetStudentByUID(uid);
            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }
            return Ok(student);
        }
    }
}