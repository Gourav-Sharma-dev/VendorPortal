using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Server.Services;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApprovalController : ControllerBase
    {
        private readonly IApprovalService _approvalService;

        public ApprovalController(IApprovalService approvalService)
        {
            _approvalService = approvalService;
        }

        /// <summary>
        /// Level 1: Procurement team approval/rejection
        /// </summary>
        [HttpPost("{vendorId}/procurement-approval")]
        [Authorize(Roles = "Admin,Procurement")]
        public async Task<IActionResult> ProcurementApproval(Guid vendorId, [FromQuery] bool isApproved, [FromQuery] string? remarks)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _approvalService.ProcurementApprovalAsync(vendorId, Guid.Parse(userId), isApproved, remarks);
            if (!result) return BadRequest("Procurement approval failed. Vendor not found or invalid status.");

            return Ok(isApproved 
                ? "Vendor approved by Procurement team and forwarded to Finance." 
                : "Vendor sent back for correction by Procurement team.");
        }

        /// <summary>
        /// Level 2: Finance team approval/rejection
        /// </summary>
        [HttpPost("{vendorId}/finance-approval")]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> FinanceApproval(Guid vendorId, [FromQuery] bool isApproved, [FromQuery] string? remarks)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _approvalService.FinanceApprovalAsync(vendorId, Guid.Parse(userId), isApproved, remarks);
            if (!result) return BadRequest("Finance approval failed. Vendor not found, not yet procurement approved, or invalid status.");

            return Ok(isApproved 
                ? "Vendor approved by Finance team. Ready for activation." 
                : "Vendor sent back for correction by Finance team.");
        }

        /// <summary>
        /// Reject vendor at any stage
        /// </summary>
        [HttpPost("{vendorId}/reject")]
        [Authorize(Roles = "Admin,Procurement,Finance")]
        public async Task<IActionResult> RejectVendor(Guid vendorId, [FromQuery] string remarks)
        {
            if (string.IsNullOrWhiteSpace(remarks))
                return BadRequest("Remarks are required for rejection.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _approvalService.RejectVendorAsync(vendorId, Guid.Parse(userId), remarks);
            if (!result) return NotFound("Vendor not found.");

            return Ok("Vendor rejected successfully.");
        }

        /// <summary>
        /// Request correction from vendor
        /// </summary>
        [HttpPost("{vendorId}/request-correction")]
        [Authorize(Roles = "Admin,Procurement,Finance")]
        public async Task<IActionResult> RequestCorrection(Guid vendorId, [FromQuery] string remarks)
        {
            if (string.IsNullOrWhiteSpace(remarks))
                return BadRequest("Remarks are required for correction request.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _approvalService.RequestCorrectionAsync(vendorId, Guid.Parse(userId), remarks);
            if (!result) return NotFound("Vendor not found.");

            return Ok("Correction requested successfully.");
        }

        /// <summary>
        /// Activate approved vendor in the system
        /// </summary>
        [HttpPost("{vendorId}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateVendor(Guid vendorId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _approvalService.ActivateVendorAsync(vendorId, Guid.Parse(userId));
            if (!result) return BadRequest("Vendor activation failed. Vendor not found or not yet approved.");

            return Ok("Vendor activated successfully.");
        }

        /// <summary>
        /// Get vendor status change history
        /// </summary>
        [HttpGet("{vendorId}/history")]
        public async Task<IActionResult> GetStatusHistory(Guid vendorId)
        {
            var history = await _approvalService.GetVendorStatusHistoryAsync(vendorId);
            return Ok(history);
        }
    }
}
