using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ContractClaimSystem.Controllers;
using Microsoft.AspNetCore.Http;

public class AccountControllerTests
{
    private readonly AccountController _controller;
    private readonly Mock<SignInManager<IdentityUser>> _mockSignInManager;
    private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
    private readonly Mock<ILogger<AccountController>> _mockLogger;

    public AccountControllerTests()
    {
        _mockUserManager = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
        _mockSignInManager = new Mock<SignInManager<IdentityUser>>(_mockUserManager.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(), null, null, null, null);
        _mockRoleManager = new Mock<RoleManager<IdentityRole>>(Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
        _mockLogger = new Mock<ILogger<AccountController>>();
        _controller = new AccountController(_mockUserManager.Object, _mockSignInManager.Object, _mockRoleManager.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Login_ReturnsRedirect_OnSuccessfulLogin()
    {
        // Arrange
        var loginModel = new LoginViewModel
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        _mockSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        // Act
        var result = await _controller.Login(loginModel) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
    }

    [Fact]
    public async Task Logout_LogsOutUser()
    {
        // Act
        var result = await _controller.Logout() as RedirectToActionResult;

        // Assert
        _mockSignInManager.Verify(m => m.SignOutAsync(), Times.Once);
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
    }

    // Add more tests for failed login, registration, etc.
}
