using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OpenUpTool.Api.Controllers;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;
using System.Security.Claims;
using Xunit;

namespace OpenUpTool.Tests.Controllers;

/// <summary>
/// Pruebas unitarias para AuthController
/// Verifica el comportamiento de login, registro y gestión de usuarios
/// </summary>
public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_authServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var loginDto = new LoginDto("test@test.com", "Test123!");
        var userDto = new UserDto(Guid.NewGuid(), "test@test.com", "Test", "User", "Admin", true, DateTime.UtcNow);
        var expectedResponse = new LoginResponseDto("test-token", userDto);
        
        _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto("test@test.com", "WrongPassword");
        _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
            .ReturnsAsync((LoginResponseDto?)null);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var registerDto = new RegisterDto("newuser@test.com", "Test123!", "New", "User", Guid.NewGuid());
        var expectedUser = new UserDto(Guid.NewGuid(), "newuser@test.com", "New", "User", "Developer", true, DateTime.UtcNow);

        _authServiceMock.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>()))
            .ReturnsAsync(expectedUser);

        // Simular autenticación con rol Admin
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        }, "mock"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new UserDto(userId, "user@test.com", "Test", "User", "Developer", true, DateTime.UtcNow);

        _authServiceMock.Setup(s => s.GetUserByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Simular autenticación
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedUser);
    }
}
