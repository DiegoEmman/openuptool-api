using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Core.Services;

public class ProjectPlanService : IProjectPlanService
{
    private readonly IProjectPlanRepository _planRepository;
    private readonly IProjectRepository _projectRepository;

    public ProjectPlanService(IProjectPlanRepository planRepository, IProjectRepository projectRepository)
    {
        _planRepository = planRepository;
        _projectRepository = projectRepository;
    }

    public async Task<ProjectPlanDto?> GetPlanByProjectAsync(Guid projectId)
    {
        var plan = await _planRepository.GetByProjectIdAsync(projectId);
        return plan == null ? null : MapToDto(plan);
    }

    public async Task<ProjectPlanDto> CreateInitialPlanAsync(Guid projectId, CreateProjectPlanDto dto)
    {
        // Verificar si ya existe un plan activo
        var existingPlans = await _planRepository.GetAllVersionsByProjectIdAsync(projectId);
        var activePlan = existingPlans.FirstOrDefault(p => p.IsActive);
        
        if (activePlan != null)
        {
            throw new InvalidOperationException("Ya existe un plan activo para el proyecto. Use CreateNewPlanVersionAsync para crear una nueva versión.");
        }

        var plan = new ProjectPlan
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Objectives = dto.Objectives.Trim(),
            Scope = dto.Scope.Trim(),
            InitialSchedule = dto.InitialSchedule.Select(s => new PhaseScheduleItem
            {
                PhaseName = s.PhaseName,
                StartDate = DateTime.SpecifyKind(s.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(s.EndDate, DateTimeKind.Utc),
                Responsible = s.Responsible?.Trim()
            }).ToList(),
            Version = 1,
            IsActive = true,
            Observations = dto.Observations?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Milestones = dto.Milestones.Select(m => new Milestone
            {
                Id = Guid.NewGuid(),
                Name = m.Name.Trim(),
                Date = DateTime.SpecifyKind(m.Date, DateTimeKind.Utc),
                Description = m.Description?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList()
        };

        var createdPlan = await _planRepository.CreateAsync(plan);

        // Actualizar la referencia del proyecto al plan
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project != null)
        {
            project.PlanId = createdPlan.Id;
            await _projectRepository.UpdateAsync(project);
        }

        return MapToDto(createdPlan);
    }

    public async Task<ProjectPlanDto> CreateNewPlanVersionAsync(Guid projectId, CreateProjectPlanDto dto)
    {
        // Obtener todas las versiones del proyecto
        var existingPlans = await _planRepository.GetAllVersionsByProjectIdAsync(projectId);
        var activePlan = existingPlans.FirstOrDefault(p => p.IsActive);
        
        if (activePlan == null)
        {
            throw new InvalidOperationException("No existe un plan activo. Use CreateInitialPlanAsync para crear v1.");
        }

        // Desactivar el plan actual
        activePlan.IsActive = false;
        await _planRepository.UpdateAsync(activePlan);

        // Crear nueva versión
        var maxVersion = existingPlans.Max(p => p.Version);
        var newPlan = new ProjectPlan
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Objectives = dto.Objectives.Trim(),
            Scope = dto.Scope.Trim(),
            InitialSchedule = dto.InitialSchedule.Select(s => new PhaseScheduleItem
            {
                PhaseName = s.PhaseName,
                StartDate = DateTime.SpecifyKind(s.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(s.EndDate, DateTimeKind.Utc),
                Responsible = s.Responsible?.Trim()
            }).ToList(),
            Version = maxVersion + 1,
            IsActive = true,
            Observations = dto.Observations?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Milestones = dto.Milestones.Select(m => new Milestone
            {
                Id = Guid.NewGuid(),
                Name = m.Name.Trim(),
                Date = DateTime.SpecifyKind(m.Date, DateTimeKind.Utc),
                Description = m.Description?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList()
        };

        var createdPlan = await _planRepository.CreateAsync(newPlan);

        // Actualizar referencia del proyecto al nuevo plan
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project != null)
        {
            project.PlanId = createdPlan.Id;
            await _projectRepository.UpdateAsync(project);
        }

        return MapToDto(createdPlan);
    }

    public async Task<IEnumerable<ProjectPlanDto>> GetPlanHistoryAsync(Guid projectId)
    {
        var plans = await _planRepository.GetAllVersionsByProjectIdAsync(projectId);
        return plans.OrderByDescending(p => p.Version).Select(MapToDto);
    }

    private static ProjectPlanDto MapToDto(ProjectPlan plan)
    {
        return new ProjectPlanDto(
            plan.Id,
            plan.ProjectId,
            plan.Objectives,
            plan.Scope,
            plan.InitialSchedule.Select(s => new PhaseScheduleItemDto(s.PhaseName, s.StartDate, s.EndDate, s.Responsible)).ToList(),
            plan.Milestones.Select(m => new MilestoneDto(m.Id, m.Name, m.Date, m.Description)).ToList(),
            plan.CreatedAt,
            plan.Version,
            plan.Observations
        );
    }
}

public class IterationService : IIterationService
{
    private readonly IIterationRepository _iterationRepository;

    public IterationService(IIterationRepository iterationRepository)
    {
        _iterationRepository = iterationRepository;
    }

    public async Task<IEnumerable<IterationDto>> GetIterationsByProjectAsync(Guid projectId)
    {
        var iterations = await _iterationRepository.GetByProjectIdAsync(projectId);
        return iterations.Select(MapToDto);
    }

    public async Task<IterationDto> CreateIterationAsync(Guid projectId, CreateIterationDto dto)
    {
        var iteration = new Iteration
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = dto.Name.Trim(),
            Objective = dto.Objective?.Trim(),
            Phase = dto.Phase,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = "Planeada"
        };

        var created = await _iterationRepository.CreateAsync(iteration);
        return MapToDto(created);
    }

    public async Task<IterationDto?> UpdateIterationStatusAsync(Guid id, string status)
    {
        var iteration = await _iterationRepository.GetByIdAsync(id);
        if (iteration == null) return null;

        iteration.Status = status;
        var updated = await _iterationRepository.UpdateAsync(iteration);
        return MapToDto(updated);
    }

    private static IterationDto MapToDto(Iteration iteration)
    {
        return new IterationDto(
            iteration.Id,
            iteration.ProjectId,
            iteration.Name,
            iteration.Objective,
            iteration.Phase,
            iteration.StartDate,
            iteration.EndDate,
            iteration.Status,
            iteration.CreatedAt
        );
    }
}
