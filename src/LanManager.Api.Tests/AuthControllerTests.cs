using LanManager.Api.Controllers;
using LanManager.Api.Tests.Helpers;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LanManager.Api.Tests;

public class AuthControllerTests
{
    private static UserManager<ApplicationUser> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        return mgr.Object;
    }

    private static IConfiguration MockConfiguration()
    {
        var config = new Dictionary<string, string>
        {
            { "Jwt:Key", "ThisIsATestKeyForJwtTokenGeneration1234567890" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" },
            { "Jwt:ExpiryMinutes", "480" }
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(config!)
            .Build();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            UserName = "testuser",
            NormalizedEmail = "TEST@EXAMPLE.COM"
        };

        var store = new Mock<IUserStore<ApplicationUser>>();
        var userManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        userManager.Setup(m => m.FindByEmailAsync("test@example.com")).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "password123")).ReturnsAsync(true);
        userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Attendee" });

        var controller = new AuthController(userManager.Object, MockConfiguration());

        var result = await controller.Login(new LoginRequest("test@example.com", "password123"));

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponse>(ok.Value);
        Assert.NotEmpty(response.Token);
        Assert.Equal(userId, response.UserId);
        Assert.Equal("Test User", response.Name);
        Assert.Equal("test@example.com", response.Email);
        Assert.Contains("Attendee", response.Roles);
    }

    [Fact]
    public async Task Login_InvalidEmail_ReturnsUnauthorized()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var userManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var controller = new AuthController(userManager.Object, MockConfiguration());

        var result = await controller.Login(new LoginRequest("invalid@example.com", "password123"));

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorized.Value);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            UserName = "testuser",
            NormalizedEmail = "TEST@EXAMPLE.COM"
        };

        var store = new Mock<IUserStore<ApplicationUser>>();
        var userManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        userManager.Setup(m => m.FindByEmailAsync("test@example.com")).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "wrongpassword")).ReturnsAsync(false);

        var controller = new AuthController(userManager.Object, MockConfiguration());

        var result = await controller.Login(new LoginRequest("test@example.com", "wrongpassword"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_UserWithMultipleRoles_ReturnsAllRoles()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Name = "Admin User",
            Email = "admin@example.com",
            UserName = "admin",
            NormalizedEmail = "ADMIN@EXAMPLE.COM"
        };

        var store = new Mock<IUserStore<ApplicationUser>>();
        var userManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        userManager.Setup(m => m.FindByEmailAsync("admin@example.com")).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "password123")).ReturnsAsync(true);
        userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin", "Organizer", "Attendee" });

        var controller = new AuthController(userManager.Object, MockConfiguration());

        var result = await controller.Login(new LoginRequest("admin@example.com", "password123"));

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponse>(ok.Value);
        Assert.Equal(3, response.Roles.Count);
        Assert.Contains("Admin", response.Roles);
        Assert.Contains("Organizer", response.Roles);
        Assert.Contains("Attendee", response.Roles);
    }
}
