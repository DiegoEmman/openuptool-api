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
/// Pruebas unitarias para ProjectsController
/// Verifica CRUD de proyectos y control de acceso
/// </summary>
public class ProjectsControllerTests
{
    private readonly Mock<IProjectService> _projectServiceMock;
    private readonly Mock<IIterationProgressService> _progressServiceMock;
    private readonly Mock<ILogger<ProjectsController>> _loggerMock;
    private readonly ProjectsController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();

    public ProjectsControllerTests()
    {
        _projectServiceMock = new Mock<IProjectService>();
        _progressServiceMock = new Mock<IIterationProgressService>();
        _loggerMock = new Mock<ILogger<ProjectsController>>();
        _controller = new ProjectsController(
            _projectServiceMock.Object, 
            _progressServiceMock.Object,
            _loggerMock.Object);

        SetupAuthenticatedUser();
    }

    private void SetupAuthenticatedUser()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Role, "Manager")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetAll_ReturnsUserProjects()
    {
        // Arrange
        var expectedProjects = new List<ProjectDto>
        {
            new ProjectDto(Guid.NewGuid(), "Project 1", "PROJ1", DateTime.UtcNow, "Creado", null, null, new List<string>(), null, new List<string>(), DateTime.UtcNow, DateTime.UtcNow),
            new ProjectDto(Guid.NewGuid(), "Project 2", "PROJ2", DateTime.UtcNow, "Creado", null, null, new List<string>(), null, new List<string>(), DateTime.UtcNow, DateTime.UtcNow)
        };

        _projectServiceMock.Setup(s => s.GetProjectsForUserAsync(_testUserId))
            .ReturnsAsync(expectedProjects);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var projects = okResult!.Value as IEnumerable<ProjectDto>;
        projects.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsProject()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var expectedProject = new ProjectDto(projectId, "Test Project", "TEST", DateTime.UtcNow, "Creado", null, null, new List<string>(), null, new List<string>(), DateTime.UtcNow, DateTime.UtcNow);

        _projectServiceMock.Setup(s => s.GetProjectByIdAsync(projectId))
            .ReturnsAsync(expectedProject);
        _projectServiceMock.Setup(s => s.HasUserAccessAsync(_testUserId, projectId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.GetById(projectId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedProject);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedProject()
    {
        // Arrange
        var createDto = new CreateProjectDto("New Project", "NEWPROJ", DateTime.UtcNow, "Test Owner", "Test description", new List<string> { "test" });
        var expectedProject = new ProjectDto(Guid.NewGuid(), "New Project", "NEWPROJ", DateTime.UtcNow, "Creado", "Test Owner", "Test description", new List<string> { "test" }, null, new List<string>(), DateTime.UtcNow, DateTime.UtcNow);

        _projectServiceMock.Setup(s => s.CreateProjectAsync(It.IsAny<CreateProjectDto>(), _testUserId))
            .ReturnsAsync(expectedProject);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedProject()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var updateDto = new UpdateProjectDto("Updated Project", "En Progreso", null, "Updated description", null);
        var updatedProject = new ProjectDto(projectId, "Updated Project", "TEST", DateTime.UtcNow, "En Progreso", null, "Updated description", new List<string>(), null, new List<string>(), DateTime.UtcNow, DateTime.UtcNow);

        _projectServiceMock.Setup(s => s.HasUserAccessAsync(_testUserId, projectId))
            .ReturnsAsync(true);
        _projectServiceMock.Setup(s => s.UpdateProjectAsync(projectId, It.IsAny<UpdateProjectDto>()))
            .ReturnsAsync(updatedProject);

        // Act
        var result = await _controller.Update(projectId, updateDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        }, "mock"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        _projectServiceMock.Setup(s => s.HasUserAccessAsync(_testUserId, projectId))
            .ReturnsAsync(true);
        _projectServiceMock.Setup(s => s.DeleteProjectAsync(projectId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(projectId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}
