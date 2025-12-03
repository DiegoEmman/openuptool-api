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
            _loggerMock.Object
        );

        // Setup User Claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithProjects()
    {
        // Arrange
        var projects = new List<ProjectDto>
        {
            new ProjectDto(
                Guid.NewGuid(),
                "Project 1",
                "PROJ1",
                DateTime.UtcNow,
                "Active",
                "Owner 1",
                "Description 1",
                new List<string> { "tag1" },
                null,
                new List<string> { "Inception" },
                DateTime.UtcNow,
                DateTime.UtcNow
            ),
            new ProjectDto(
                Guid.NewGuid(),
                "Project 2",
                "PROJ2",
                DateTime.UtcNow,
                "Active",
                "Owner 2",
                "Description 2",
                new List<string> { "tag2" },
                null,
                new List<string> { "Elaboration" },
                DateTime.UtcNow,
                DateTime.UtcNow
            )
        };

        _projectServiceMock
            .Setup(s => s.GetProjectsForUserAsync(_testUserId))
            .ReturnsAsync(projects);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProjects = Assert.IsAssignableFrom<IEnumerable<ProjectDto>>(okResult.Value);
        Assert.Equal(2, returnedProjects.Count());
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithProject()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = new ProjectDto(
            projectId,
            "Test Project",
            "TEST1",
            DateTime.UtcNow,
            "Active",
            "Test Owner",
            "Test Description",
            new List<string> { "test" },
            null,
            new List<string> { "Inception" },
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _projectServiceMock
            .Setup(s => s.GetProjectByIdAsync(projectId))
            .ReturnsAsync(project);
        
        _projectServiceMock
            .Setup(s => s.HasUserAccessAsync(_testUserId, projectId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.GetById(projectId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProject = Assert.IsType<ProjectDto>(okResult.Value);
        Assert.Equal(projectId, returnedProject.Id);
        Assert.Equal("Test Project", returnedProject.Name);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        _projectServiceMock
            .Setup(s => s.GetProjectByIdAsync(projectId))
            .ReturnsAsync((ProjectDto?)null);

        _projectServiceMock
            .Setup(s => s.HasUserAccessAsync(_testUserId, projectId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.GetById(projectId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedProject()
    {
        // Arrange
        var createDto = new CreateProjectDto(
            "New Project",
            "NEWPROJ",
            DateTime.UtcNow,
            "Test Owner",
            "New Description",
            new List<string> { "newtag" }
        );

        var createdProject = new ProjectDto(
            Guid.NewGuid(),
            createDto.Name,
            createDto.Identifier,
            createDto.StartDate,
            "Active",
            createDto.Owner,
            createDto.Description,
            createDto.Tags,
            null,
            new List<string> { "Inception" },
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _projectServiceMock
            .Setup(s => s.CreateProjectAsync(It.IsAny<CreateProjectDto>(), _testUserId))
            .ReturnsAsync(createdProject);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedProject = Assert.IsType<ProjectDto>(createdResult.Value);
        Assert.Equal(createDto.Name, returnedProject.Name);
    }
}
