using FluentAssertions;
using Moq;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using OpenUpTool.Core.Services;
using Xunit;

namespace OpenUpTool.Tests.Services;

/// <summary>
/// Pruebas unitarias para ProjectService
/// Verifica la lógica de negocio de gestión de proyectos
/// </summary>
public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IPhaseRepository> _phaseRepositoryMock;
    private readonly Mock<IProjectUserRoleRepository> _projectUserRoleRepositoryMock;
    private readonly ProjectService _service;

    public ProjectServiceTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _phaseRepositoryMock = new Mock<IPhaseRepository>();
        _projectUserRoleRepositoryMock = new Mock<IProjectUserRoleRepository>();
        
        _service = new ProjectService(
            _projectRepositoryMock.Object,
            _phaseRepositoryMock.Object,
            _projectUserRoleRepositoryMock.Object
        );
    }

    [Fact]
    public async Task GetAllProjectsAsync_ReturnsAllProjects()
    {
        // Arrange
        var projects = new List<Project>
        {
            new Project { Id = Guid.NewGuid(), Name = "Project 1", Identifier = "PROJ1", Status = "Creado", StartDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Project { Id = Guid.NewGuid(), Name = "Project 2", Identifier = "PROJ2", Status = "Creado", StartDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _projectRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(projects);

        // Act
        var result = await _service.GetAllProjectsAsync();

        // Assert
        result.Should().HaveCount(2);
        _projectRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetProjectByIdAsync_WithValidId_ReturnsProject()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = new Project 
        { 
            Id = projectId, 
            Name = "Test Project", 
            Identifier = "TEST",
            Status = "Creado",
            StartDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        // Act
        var result = await _service.GetProjectByIdAsync(projectId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(projectId);
        result.Name.Should().Be("Test Project");
    }

    [Fact]
    public async Task HasUserAccessAsync_WithAccess_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _projectUserRoleRepositoryMock.Setup(r => r.HasUserAccessToProjectAsync(userId, projectId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.HasUserAccessAsync(userId, projectId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CreateProjectAsync_CreatesProjectWithPhases()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = new CreateProjectDto("New Project", "NEWPROJ", DateTime.UtcNow, "Test Owner", "Test description", new List<string> { "test" });

        var createdProject = new Project
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Identifier = createDto.Identifier,
            StartDate = createDto.StartDate,
            Status = "Creado",
            Owner = createDto.Owner,
            Description = createDto.Description,
            Tags = createDto.Tags,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Phases = new List<Phase>()
        };

        _projectRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Project>()))
            .ReturnsAsync(createdProject);
        _phaseRepositoryMock.Setup(r => r.CreateManyAsync(It.IsAny<IEnumerable<Phase>>()))
            .ReturnsAsync(new List<Phase>());
        _projectUserRoleRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<ProjectUserRole>()))
            .ReturnsAsync(new ProjectUserRole());
        _projectRepositoryMock.Setup(r => r.GetByIdAsync(createdProject.Id))
            .ReturnsAsync(createdProject);

        // Act
        var result = await _service.CreateProjectAsync(createDto, userId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createDto.Name);
        
        // Verificar que se crearon las 4 fases de OpenUP
        _phaseRepositoryMock.Verify(r => r.CreateManyAsync(
            It.Is<IEnumerable<Phase>>(phases => phases.Count() == 4)), 
            Times.Once);
    }

    [Fact]
    public async Task DeleteProjectAsync_CallsRepositoryDelete()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _projectRepositoryMock.Setup(r => r.DeleteAsync(projectId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteProjectAsync(projectId);

        // Assert
        _projectRepositoryMock.Verify(r => r.DeleteAsync(projectId), Times.Once);
    }
}
