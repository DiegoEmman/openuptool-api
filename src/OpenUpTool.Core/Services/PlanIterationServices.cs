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
            Status = "Planeada",
            PlannedCapacityHours = dto.PlannedCapacityHours,
            TeamSize = dto.TeamSize,
            PlannedPoints = dto.PlannedPoints
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

    // HU-016: Actualizar capacidad del equipo
    public async Task<IterationDto?> UpdateIterationCapacityAsync(Guid id, UpdateIterationCapacityDto dto)
    {
        var iteration = await _iterationRepository.GetByIdAsync(id);
        if (iteration == null) return null;

        if (dto.PlannedCapacityHours.HasValue)
            iteration.PlannedCapacityHours = dto.PlannedCapacityHours;
        if (dto.TeamSize.HasValue)
            iteration.TeamSize = dto.TeamSize;
        if (dto.PlannedPoints.HasValue)
            iteration.PlannedPoints = dto.PlannedPoints;

        iteration.UpdatedAt = DateTime.UtcNow;
        var updated = await _iterationRepository.UpdateAsync(iteration);
        return MapToDto(updated);
    }

    // HU-016: Registrar velocidad (puntos completados)
    public async Task<IterationDto?> UpdateIterationVelocityAsync(Guid id, UpdateIterationVelocityDto dto)
    {
        var iteration = await _iterationRepository.GetByIdAsync(id);
        if (iteration == null) return null;

        iteration.PlannedPoints = dto.PlannedPoints;
        iteration.CompletedPoints = dto.CompletedPoints;
        iteration.UpdatedAt = DateTime.UtcNow;
        var updated = await _iterationRepository.UpdateAsync(iteration);
        return MapToDto(updated);
    }

    // HU-016: Obtener estadísticas de velocidad del proyecto
    public async Task<ProjectVelocityStatsDto?> GetProjectVelocityStatsAsync(Guid projectId)
    {
        var iterations = (await _iterationRepository.GetByProjectIdAsync(projectId)).ToList();
        if (!iterations.Any()) return null;

        // Contar todas las iteraciones que tienen datos de puntos completados
        var iterationsWithData = iterations
            .Where(i => i.CompletedPoints.HasValue && i.CompletedPoints > 0)
            .ToList();

        var avgVelocity = iterationsWithData.Any() 
            ? (decimal)iterationsWithData.Average(i => i.CompletedPoints ?? 0) 
            : 0;

        var avgCapacity = iterationsWithData.Any() && iterationsWithData.Any(i => i.PlannedCapacityHours.HasValue)
            ? (decimal)iterationsWithData.Where(i => i.PlannedCapacityHours.HasValue).Average(i => i.PlannedCapacityHours ?? 0)
            : 0;

        var avgTeamSize = iterationsWithData.Any() && iterationsWithData.Any(i => i.TeamSize.HasValue)
            ? (decimal)iterationsWithData.Where(i => i.TeamSize.HasValue).Average(i => i.TeamSize ?? 0)
            : 0;

        // Sugerencia basada en promedio de las últimas 3 iteraciones
        var lastThree = iterationsWithData.OrderByDescending(i => i.EndDate).Take(3).ToList();
        var suggestedPoints = lastThree.Any() 
            ? (decimal)lastThree.Average(i => i.CompletedPoints ?? 0) 
            : avgVelocity;

        var totalCompletedPoints = iterationsWithData.Sum(i => i.CompletedPoints ?? 0);

        var project = iterations.First().Project;

        return new ProjectVelocityStatsDto(
            projectId,
            project?.Name ?? "Proyecto",
            iterations.Count,
            iterationsWithData.Count,
            Math.Round(avgVelocity, 1),
            Math.Round(avgCapacity, 1),
            Math.Round(avgTeamSize, 1),
            Math.Round(suggestedPoints, 0),
            totalCompletedPoints,
            iterations.OrderByDescending(i => i.EndDate).Select(MapToVelocityDto).ToList()
        );
    }

    // HU-016: Obtener datos para planificación
    public async Task<PlanningDataDto?> GetPlanningDataAsync(Guid projectId)
    {
        var iterations = (await _iterationRepository.GetByProjectIdAsync(projectId)).ToList();
        if (!iterations.Any()) return null;

        var iterationsWithData = iterations
            .Where(i => i.CompletedPoints.HasValue && i.CompletedPoints > 0)
            .OrderByDescending(i => i.EndDate)
            .ToList();

        var avgVelocity = iterationsWithData.Any() 
            ? (decimal)iterationsWithData.Average(i => i.CompletedPoints ?? 0) 
            : 0;

        var avgCapacity = iterationsWithData.Any() && iterationsWithData.Any(i => i.PlannedCapacityHours.HasValue)
            ? (decimal)iterationsWithData.Where(i => i.PlannedCapacityHours.HasValue).Average(i => i.PlannedCapacityHours ?? 0)
            : 0;

        // Sugerencia: promedio de últimas 3 iteraciones o promedio general
        var lastThree = iterationsWithData.Take(3).ToList();
        var suggestedPoints = lastThree.Any() 
            ? (decimal)lastThree.Average(i => i.CompletedPoints ?? 0) 
            : avgVelocity;

        var lastTeamSize = iterationsWithData.FirstOrDefault()?.TeamSize;

        // Generar recomendación
        string recommendation;
        if (!iterationsWithData.Any())
        {
            recommendation = "Sin datos historicos. Inicie con una estimacion conservadora y ajuste segun avance.";
        }
        else if (iterationsWithData.Count < 3)
        {
            recommendation = $"Datos limitados ({iterationsWithData.Count} iteracion(es)). Considere {Math.Round(suggestedPoints)} puntos. Ajuste segun capacidad real.";
        }
        else
        {
            recommendation = $"Basado en {iterationsWithData.Count} iteraciones, se sugiere planificar {Math.Round(suggestedPoints)} puntos para un alcance realista.";
        }

        var project = iterations.First().Project;

        return new PlanningDataDto(
            projectId,
            project?.Name ?? "Proyecto",
            Math.Round(avgVelocity, 1),
            Math.Round(avgCapacity, 1),
            Math.Round(suggestedPoints, 0),
            lastTeamSize,
            recommendation,
            iterationsWithData.Take(5).Select(i => new IterationVelocitySummaryDto(
                i.Name,
                i.PlannedPoints,
                i.CompletedPoints,
                i.PlannedPoints.HasValue && i.PlannedPoints > 0 && i.CompletedPoints.HasValue
                    ? Math.Round((decimal)i.CompletedPoints.Value / i.PlannedPoints.Value * 100, 1)
                    : null
            )).ToList()
        );
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
            iteration.PlannedCapacityHours,
            iteration.TeamSize,
            iteration.PlannedPoints,
            iteration.CompletedPoints,
            iteration.CreatedAt
        );
    }

    private static IterationVelocityDto MapToVelocityDto(Iteration iteration)
    {
        decimal? velocityPerHour = iteration.PlannedCapacityHours.HasValue && iteration.PlannedCapacityHours > 0 && iteration.CompletedPoints.HasValue
            ? Math.Round((decimal)iteration.CompletedPoints.Value / iteration.PlannedCapacityHours.Value, 2)
            : null;

        decimal? pointsPerMember = iteration.TeamSize.HasValue && iteration.TeamSize > 0 && iteration.CompletedPoints.HasValue
            ? Math.Round((decimal)iteration.CompletedPoints.Value / iteration.TeamSize.Value, 2)
            : null;

        return new IterationVelocityDto(
            iteration.Id,
            iteration.Name,
            iteration.Phase,
            iteration.StartDate,
            iteration.EndDate,
            iteration.Status,
            iteration.PlannedCapacityHours,
            iteration.TeamSize,
            iteration.PlannedPoints,
            iteration.CompletedPoints,
            velocityPerHour,
            pointsPerMember
        );
    }
}
