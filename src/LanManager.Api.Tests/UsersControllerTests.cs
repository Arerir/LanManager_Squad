using LanManager.Api.Controllers;
using LanManager.Api.DTOs;
using LanManager.Api.Tests.Helpers;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LanManager.Api.Tests;

public class UsersControllerTests
{
    private static UserManager<ApplicationUser> MockUserManager(
        ApplicationUser? findByIdReturn = null,
        IdentityResult? createReturn = null,
        List<ApplicationUser>? allUsers = null)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        if (findByIdReturn != null)
            mgr.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(findByIdReturn);
        
        if (createReturn != null)
            mgr.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(createReturn);
        
        if (allUsers != null)
        {
            var queryable = allUsers.AsQueryable();
            mgr.Setup(m => m.Users).Returns(queryable);
        }

        return mgr.Object;
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsCreatedUser()
    {
        var userManager = MockUserManager(createReturn: IdentityResult.Success);
        var controller = new UsersController(userManager);

        var request = new CreateUserRequest(
            "John Doe",
            "johndoe",
            "john@example.com",
            "Password123!"
        );

        var result = await controller.Register(request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<UserDto>(created.Value);
        Assert.Equal("John Doe", dto.Name);
        Assert.Equal("johndoe", dto.UserName);
    }

    [Fact]
    public async Task Register_InvalidPassword_ReturnsValidationProblem()
    {
        var errors = new[]
        {
            new IdentityError { Code = "PasswordTooShort", Description = "Password must be at least 6 characters." }
        };
        var userManager = MockUserManager(createReturn: IdentityResult.Failed(errors));
        var controller = new UsersController(userManager);

        var request = new CreateUserRequest("Name", "username", "email@test.com", "123");

        var result = await controller.Register(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_ExistingUser_ReturnsUser()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId, Name = "Test User", UserName = "testuser", Email = "test@example.com"
        };
        var userManager = MockUserManager(findByIdReturn: user);
        var controller = new UsersController(userManager);
        ClaimsHelper.SetUser(controller, Guid.NewGuid());

        var result = await controller.GetById(userId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<UserDto>(ok.Value);
        Assert.Equal(userId, dto.Id);
        Assert.Equal("Test User", dto.Name);
    }

    [Fact]
    public async Task GetById_NonExistentUser_ReturnsNotFound()
    {
        var userManager = MockUserManager(findByIdReturn: null);
        var controller = new UsersController(userManager);
        ClaimsHelper.SetUser(controller, Guid.NewGuid());

        var result = await controller.GetById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetAll_DefaultPagination_ReturnsFirstPage()
    {
        using var db = TestDbContextFactory.Create("Users_GetAll_Default");
        
        db.Users.Add(new ApplicationUser 
        { 
            Id = Guid.NewGuid(), 
            Name = "User A", 
            UserName = "usera", 
            Email = "a@test.com",
            NormalizedUserName = "USERA",
            NormalizedEmail = "A@TEST.COM"
        });
        db.Users.Add(new ApplicationUser 
        { 
            Id = Guid.NewGuid(), 
            Name = "User B", 
            UserName = "userb", 
            Email = "b@test.com",
            NormalizedUserName = "USERB",
            NormalizedEmail = "B@TEST.COM"
        });
        db.Users.Add(new ApplicationUser 
        { 
            Id = Guid.NewGuid(), 
            Name = "User C", 
            UserName = "userc", 
            Email = "c@test.com",
            NormalizedUserName = "USERC",
            NormalizedEmail = "C@TEST.COM"
        });
        await db.SaveChangesAsync();

        var store = new Mock<IUserStore<ApplicationUser>>();
        var userManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        userManager.Setup(m => m.Users).Returns(db.Users);

        var controller = new UsersController(userManager.Object);
        ClaimsHelper.SetUser(controller, Guid.NewGuid(), "Admin");

        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var pagedResult = Assert.IsType<PagedResult<UserDto>>(ok.Value);
        Assert.Equal(3, pagedResult.TotalCount);
        Assert.Equal(1, pagedResult.Page);
        Assert.Equal(20, pagedResult.PageSize);
    }

    [Fact]
    public async Task GetAll_CustomPageSize_ReturnsCorrectPage()
    {
        using var db = TestDbContextFactory.Create("Users_GetAll_CustomPage");
        
        for (int i = 1; i <= 50; i++)
        {
            db.Users.Add(new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Name = $"User {i}",
                UserName = $"user{i}",
                Email = $"user{i}@test.com",
                NormalizedUserName = $"USER{i}",
                NormalizedEmail = $"USER{i}@TEST.COM"
            });
        }
        await db.SaveChangesAsync();

        var store = new Mock<IUserStore<ApplicationUser>>();
        var userManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        userManager.Setup(m => m.Users).Returns(db.Users);

        var controller = new UsersController(userManager.Object);
        ClaimsHelper.SetUser(controller, Guid.NewGuid(), "Admin");

        var result = await controller.GetAll(page: 2, pageSize: 10);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var pagedResult = Assert.IsType<PagedResult<UserDto>>(ok.Value);
        Assert.Equal(50, pagedResult.TotalCount);
        Assert.Equal(2, pagedResult.Page);
        Assert.Equal(10, pagedResult.PageSize);
        Assert.Equal(10, pagedResult.Items.Count());
    }
}
