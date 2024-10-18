using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractClaimSystem.Models;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

[Authorize]  // Ensure users are authenticated for all actions in this controller
public class ClaimsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClaimsController> _logger;

    public ClaimsController(ApplicationDbContext context, ILogger<ClaimsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Claims/SubmitClaim - For lecturers to submit claims
    [HttpGet]
    [Authorize(Roles = "Lecturer")]  // Only Lecturers can submit claims
    public IActionResult SubmitClaim()
    {
        // Populate LecturerName with currently logged-in user's name
        ViewBag.LecturerName = User.Identity.Name;
        return View();
    }

    // POST: Claims/SubmitClaim - Handles the submission of a claim by a lecturer
    [HttpPost]
    [Authorize(Roles = "Lecturer")]
    public async Task<IActionResult> SubmitClaim(ClaimSubmission claim, IFormFile SupportingDocument)
    {
        // Auto-populate LecturerName from the logged-in user
        claim.LecturerName = User.Identity.Name;

        if (SupportingDocument != null && SupportingDocument.Length > 0)
        {
            // If a file is uploaded, process it
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", SupportingDocument.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await SupportingDocument.CopyToAsync(stream);
            }

            // Save the file path to the claim object
            claim.SupportingDocument = $"/uploads/{SupportingDocument.FileName}";
        }
        else
        {
            // If no file is uploaded, skip file handling
            _logger.LogWarning("No file was uploaded. SupportingDocument is optional.");
            claim.SupportingDocument = null; // This ensures the field is not validated as required
        }

        if (SupportingDocument != null && SupportingDocument.Length > 0)
        {
            _logger.LogInformation($"File received: {SupportingDocument.FileName}");
        }
        else
        {
            _logger.LogWarning("No file uploaded.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Set initial status as "Pending"
                claim.Status = "Pending";

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                return RedirectToAction("ClaimSubmitted");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving claim to database: {ex.Message}");
            }
        }

        // Log validation errors
        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            _logger.LogWarning($"Validation error: {error.ErrorMessage}");
        }

        return View(claim);
    }


    // Displays the claim submission confirmation page
    public IActionResult ClaimSubmitted()
    {
        return View();
    }

    // GET: Claims/ManageClaims - For academic managers to review and approve/reject claims
    [HttpGet]
    [Authorize(Roles = "AcademicManager")]  // Only Academic Managers can manage claims
    public async Task<IActionResult> ManageClaims()
    {
        var pendingClaims = await _context.Claims.Where(c => c.Status == "Pending").ToListAsync();
        return View(pendingClaims);
    }

    // POST: Claims/ApproveClaim - Academic managers approve claims
    [HttpPost]
    [Authorize(Roles = "AcademicManager")]  // Only Academic Managers can approve claims
    public async Task<IActionResult> ApproveClaim(int claimId)
    {
        var claim = await _context.Claims.FindAsync(claimId);
        if (claim == null)
        {
            return NotFound();
        }

        claim.Status = "Approved";
        await _context.SaveChangesAsync();

        return RedirectToAction("ManageClaims");
    }

    // POST: Claims/RejectClaim - Academic managers reject claims
    [HttpPost]
    [Authorize(Roles = "AcademicManager")]  // Only Academic Managers can reject claims
    public async Task<IActionResult> RejectClaim(int claimId)
    {
        var claim = await _context.Claims.FindAsync(claimId);
        if (claim == null)
        {
            return NotFound();
        }

        claim.Status = "Rejected";
        await _context.SaveChangesAsync();

        return RedirectToAction("ManageClaims");
    }

    // GET: Claims/ApprovedClaims - Academic managers view approved claims
    [HttpGet]
    [Authorize(Roles = "AcademicManager")]  // Only Academic Managers can view approved claims
    public async Task<IActionResult> ApprovedClaims()
    {
        var approvedClaims = await _context.Claims.Where(c => c.Status == "Approved").ToListAsync();
        return View(approvedClaims);
    }

    // GET: Claims/RejectedClaims - Academic managers view rejected claims
    [HttpGet]
    [Authorize(Roles = "AcademicManager")]  // Only Academic Managers can view rejected claims
    public async Task<IActionResult> RejectedClaims()
    {
        var rejectedClaims = await _context.Claims.Where(c => c.Status == "Rejected").ToListAsync();
        return View(rejectedClaims);
    }

    [HttpGet]
    [Authorize(Roles = "Lecturer")]
    public async Task<IActionResult> UploadDocument()
    {
        // Get pending claims for the logged-in lecturer
        var lecturerName = User.Identity.Name;
        var pendingClaims = await _context.Claims
                                          .Where(c => c.LecturerName == lecturerName && c.Status == "Pending")
                                          .ToListAsync();
        return View(pendingClaims);
    }

    [HttpPost]
    [Authorize(Roles = "Lecturer")]
    public async Task<IActionResult> UploadAdditionalFiles(int claimId, List<IFormFile> additionalFiles)
    {
        var claim = await _context.Claims.FindAsync(claimId);
        if (claim == null || claim.LecturerName != User.Identity.Name)
        {
            return NotFound();
        }

        // Process each additional file
        foreach (var file in additionalFiles)
        {
            if (file.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                // Add the file path to the claim's AdditionalUploads list
                claim.AdditionalUploads.Add($"/uploads/{file.FileName}");
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("UploadDocument");
    }


}
