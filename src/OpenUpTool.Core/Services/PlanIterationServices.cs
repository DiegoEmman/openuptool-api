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
        var existing = await _planRepository.GetByProjectIdAsync(projectId);
        if (existing != null)
        {
            throw new InvalidOperationException("Plan inicial ya existe para el proyecto");
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
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                Responsible = s.Responsible?.Trim()
            }).ToList(),
            Version = 1,
            Observations = dto.Observations?.Trim(),
            Milestones = dto.Milestones.Select(m => new Milestone
            {
                Id = Guid.NewGuid(),
                Name = m.Name.Trim(),
                Date = m.Date,
                Description = m.Description?.Trim()
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
