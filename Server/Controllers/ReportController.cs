using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Model.DTO.VendorsDto;
using Server.Services;


namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Procurement,Finance")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("vendor-registration")]
        public async Task<IActionResult> GetVendorRegistrationReport([FromQuery] ReportFilter filter)
        {
            var report = await _reportService.GetVendorRegistrationReportAsync(filter);
            return Ok(report);
        }

        [HttpGet("vendor-verification")]
        public async Task<IActionResult> GetVendorVerificationReport([FromQuery] ReportFilter filter)
        {
            var report = await _reportService.GetVendorVerificationReportAsync(filter);
            return Ok(report);
        }
    }
}
