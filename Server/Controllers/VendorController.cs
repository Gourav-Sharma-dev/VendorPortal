using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Server.Model.Enums;
using Server.Model.DTO.VendorsDto;
using Server.Services;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController : ControllerBase
    {
        private readonly IVendorService _vendorService;
        private readonly string _uploadPath;

        public VendorController(IVendorService vendorService, IConfiguration configuration)
        {
            _vendorService = vendorService;
            // Read the path directly from configuration
            var relativePath = configuration["UploadSettings:VendorDocumentsPath"];
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
        }

        /// <summary>
        /// Get vendor details by ID
        /// </summary>
        [HttpGet("{vendorId}")]
        [Authorize]
        public async Task<IActionResult> GetVendor(Guid vendorId)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
            if (vendor == null) return NotFound("Vendor not found.");
            return Ok(vendor);
        }

        /// <summary>
        /// Get all vendors (Admin, Procurement, Finance only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Procurement,Finance")]
        public async Task<IActionResult> GetAllVendors()
        {
            var vendors = await _vendorService.GetAllVendorsAsync();
            return Ok(vendors);
        }

        /// <summary>
        /// Get only the vendors registered by the currently logged-in vendor user
        /// </summary>
        [HttpGet("mylist")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetMyVendors()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var vendors = await _vendorService.GetMyVendorsAsync(userId);
            return Ok(vendors);
        }

        /// <summary>
        /// Get vendors by status
        /// </summary>
        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin,Procurement,Finance")]
        public async Task<IActionResult> GetVendorsByStatus(VendorStatus status)
        {
            var vendors = await _vendorService.GetVendorsByStatusAsync(status);
            return Ok(vendors);
        }

        /// <summary>
        /// Register a new vendor
        /// </summary>
        [HttpPost("register")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> RegisterVendor([FromBody] VendorRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var vendorId = await _vendorService.RegisterVendorAsync(userId, request);
            return Ok(new
            {
                VendorId = vendorId,
                Message = "Vendor registered successfully. Please upload mandatory documents (GST Certificate and PAN Card)."
            });
        }

        /// <summary>
        /// Update vendor information (only allowed in certain statuses)
        /// </summary>
        [HttpPut("{vendorId}")]
        [Authorize]
        public async Task<IActionResult> UpdateVendor(Guid vendorId, [FromBody] VendorRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _vendorService.UpdateVendorAsync(userId, vendorId, request);
            if (!result)
                return BadRequest("Vendor update failed. Vendor not found or cannot be edited in current status.");

            return Ok(new { message = "Vendor updated successfully." });
        }

        /// <summary>
        /// Upload document for vendor
        /// </summary>
        [HttpPost("{vendorId}/upload-document")]
        [Authorize]
        public async Task<IActionResult> UploadDocument(Guid vendorId, [FromForm] string documentType, [FromForm] IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var result = await _vendorService.UploadDocumentAsync(userId, vendorId, documentType, file,_uploadPath);
            if (!result)
                return BadRequest(new { message = "Document upload failed. Invalid file type or document type."});

            return Ok(new { message = "Document uploaded successfully."});
        }

        /// <summary>
        /// Get all documents for a vendor
        /// </summary>
        [HttpGet("{vendorId}/documents")]
        [Authorize]
        public async Task<IActionResult> GetVendorDocuments(Guid vendorId)
        {
            var documents = await _vendorService.GetVendorDocumentsAsync(vendorId);
            return Ok(documents);
        }
        /// <summary>
        /// Download a specific document by filename
        /// </summary>
        [HttpGet("download/{fileName}")]
        [Authorize]
        public IActionResult Download(string fileName)
        {
            var filePath = Path.Combine(_uploadPath, fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "File not found." });

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            var contentType = ext switch
            {
                ".pdf"  => "application/pdf",
                ".png"  => "image/png",
                ".jpg"  => "image/jpeg",
                ".jpeg" => "image/jpeg",
                _       => "application/octet-stream"
            };
            return PhysicalFile(filePath, contentType, fileName);
        }
    }
}