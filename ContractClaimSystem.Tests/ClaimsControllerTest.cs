using Xunit;
using Moq;
using ContractClaimSystem.Controllers;
using ContractClaimSystem.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ClaimsControllerTests
{
    private readonly ClaimsController _controller;
    private readonly Mock<ApplicationDbContext> _mockDbContext;
    private readonly Mock<ILogger<ClaimsController>> _mockLogger;

    public ClaimsControllerTests()
    {
        _mockDbContext = new Mock<ApplicationDbContext>();
        _mockLogger = new Mock<ILogger<ClaimsController>>();
        _controller = new ClaimsController(_mockDbContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SubmitClaim_ReturnsRedirectToClaimSubmitted_OnValidClaim()
    {
        // Arrange
        var claim = new ClaimSubmission
        {
            LecturerName = "Test Lecturer",
            HoursWorked = 10,
            HourlyRate = 20,
            AdditionalNotes = "Some notes"
        };

        // Act
        var result = await _controller.SubmitClaim(claim, null) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ClaimSubmitted", result.ActionName);
    }

    [Fact]
    public async Task SubmitClaim_ReturnsView_OnInvalidModel()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Model Error");

        var claim = new ClaimSubmission
        {
            LecturerName = "Test Lecturer"
        };

        // Act
        var result = await _controller.SubmitClaim(claim, null) as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(claim, result.Model);
    }

    [Fact]
    public async Task ManageClaims_ReturnsPendingClaims()
    {
        // Arrange
        var claims = new List<ClaimSubmission>
        {
            new ClaimSubmission { Status = "Pending" },
            new ClaimSubmission { Status = "Pending" }
        };

        var mockSet = new Mock<DbSet<ClaimSubmission>>();
        _mockDbContext.Setup(m => m.Claims).Returns(mockSet.Object);

        // Act
        var result = await _controller.ManageClaims() as ViewResult;

        // Assert
        Assert.NotNull(result);
        var model = Assert.IsType<List<ClaimSubmission>>(result.Model);
        Assert.Equal(2, model.Count);
    }

    // Add more tests for ApproveClaim and RejectClaim
}