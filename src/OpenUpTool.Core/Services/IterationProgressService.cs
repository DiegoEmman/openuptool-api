using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Core.Services;

public class IterationProgressService : IIterationProgressService
{
    private readonly IIterationProgressRepository _progressRepository;
    private readonly IIterationRepository _iterationRepository;
    private readonly IIterationTaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;

    public IterationProgressService(
        IIterationProgressRepository progressRepository,
        IIterationRepository iterationRepository,
        IIterationTaskRepository taskRepository,
        IProjectRepository projectRepository)
    {
        _progressRepository = progressRepository;
        _iterationRepository = iterationRepository;
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
    }

    public async Task<IEnumerable<IterationProgressDto>> GetProgressByIterationAsync(Guid iterationId)
    {
        var progress = await _progressRepository.GetByIterationIdAsync(iterationId);
        return progress.Select(MapToDto);
    }

    public async Task<IterationProgressDto?> GetLatestProgressAsync(Guid iterationId)
    {
        var progress = await _progressRepository.GetLatestByIterationIdAsync(iterationId);
        return progress == null ? null : MapToDto(progress);
    }

    public async Task<IterationProgressDto> CreateProgressRecordAsync(Guid iterationId, CreateIterationProgressDto dto)
    {
        var iteration = await _iterationRepository.GetByIdAsync(iterationId);
        if (iteration == null)
            throw new InvalidOperationException("Iteración no encontrada");

        var progress = new IterationProgress
        {
            Id = Guid.NewGuid(),
            IterationId = iterationId,
            RecordDate = DateTime.SpecifyKind(dto.RecordDate, DateTimeKind.Utc),
            CompletionPercentage = dto.CompletionPercentage,
            TotalTasks = dto.TotalTasks,
            CompletedTasks = dto.CompletedTasks,
            InProgressTasks = dto.InProgressTasks,
            BlockedTasks = dto.BlockedTasks,
            Blockers = dto.Blockers?.Trim(),
            Observations = dto.Observations?.Trim()
        };

        var created = await _progressRepository.CreateAsync(progress);
        return MapToDto(created);
    }

    public async Task<IterationProgressDto?> UpdateProgressRecordAsync(Guid id, UpdateIterationProgressDto dto)
    {
        var progress = await _progressRepository.GetByIdAsync(id);
        if (progress == null)
            return null;

        if (dto.CompletionPercentage.HasValue)
            progress.CompletionPercentage = dto.CompletionPercentage.Value;

        if (dto.TotalTasks.HasValue)
            progress.TotalTasks = dto.TotalTasks.Value;

        if (dto.CompletedTasks.HasValue)
            progress.CompletedTasks = dto.CompletedTasks.Value;

        if (dto.InProgressTasks.HasValue)
            progress.InProgressTasks = dto.InProgressTasks.Value;

        if (dto.BlockedTasks.HasValue)
            progress.BlockedTasks = dto.BlockedTasks.Value;

        if (dto.Blockers != null)
            progress.Blockers = dto.Blockers.Trim();

        if (dto.Observations != null)
            progress.Observations = dto.Observations.Trim();

        var updated = await _progressRepository.UpdateAsync(progress);
        return MapToDto(updated);
    }

    public async Task<IterationSummaryDto?> GetIterationSummaryAsync(Guid iterationId)
    {
        var iteration = await _iterationRepository.GetByIdAsync(iterationId);
        if (iteration == null)
            return null;

        var latestProgress = await _progressRepository.GetLatestByIterationIdAsync(iterationId);
        var tasks = await _taskRepository.GetByIterationIdAsync(iterationId);

        var totalTasks = tasks.Count();
        var completedTasks = tasks.Count(t => t.Status == "completed");
        var inProgressTasks = tasks.Count(t => t.Status == "in_progress");
        var blockedTasks = tasks.Count(t => t.Status == "blocked");

        var completionPercentage = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0;

        return new IterationSummaryDto(
            iteration.Id,
            iteration.Name,
            iteration.Phase,
            iteration.StartDate,
            iteration.EndDate,
            iteration.Status,
            latestProgress?.CompletionPercentage ?? completionPercentage,
            totalTasks,
            completedTasks,
            inProgressTasks,
            blockedTasks,
            latestProgress == null ? null : MapToDto(latestProgress)
        );
    }

    public async Task<ProjectDashboardDto?> GetProjectDashboardAsync(Guid projectId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            return null;

        var iterations = await _iterationRepository.GetByProjectIdAsync(projectId);
        var iterationList = iterations.ToList();

        // Calcular progreso por fase
        var phases = new[] { "INCEPTION", "ELABORATION", "CONSTRUCTION", "TRANSITION" };
        var phaseProgressList = new List<PhaseProgressDto>();

        foreach (var phaseCode in phases)
        {
            var phaseIterations = iterationList.Where(i => i.Phase == phaseCode).ToList();
            var totalIterations = phaseIterations.Count;
            var completedIterations = phaseIterations.Count(i => i.Status == "Finalizada");
            var completionPercentage = totalIterations > 0 ? (decimal)completedIterations / totalIterations * 100 : 0;

            phaseProgressList.Add(new PhaseProgressDto(
                phaseCode,
                GetPhaseName(phaseCode),
                completionPercentage,
                totalIterations,
                completedIterations
            ));
        }

        // Calcular progreso general del proyecto
        var totalProjectIterations = iterationList.Count;
        var completedProjectIterations = iterationList.Count(i => i.Status == "Finalizada");
        var overallCompletion = totalProjectIterations > 0 ? (decimal)completedProjectIterations / totalProjectIterations * 100 : 0;

        // Obtener iteraciones recientes con su progreso
        var recentIterations = new List<IterationSummaryDto>();
        foreach (var iteration in iterationList.OrderByDescending(i => i.StartDate).Take(5))
        {
            var summary = await GetIterationSummaryAsync(iteration.Id);
            if (summary != null)
                recentIterations.Add(summary);
        }

        return new ProjectDashboardDto(
            project.Id,
            project.Name,
            overallCompletion,
            phaseProgressList,
            recentIterations
        );
    }

    public async Task<BurndownDataDto?> GetBurndownDataAsync(Guid iterationId)
    {
        var iteration = await _iterationRepository.GetByIdAsync(iterationId);
        if (iteration == null)
            return null;

        var progressRecords = await _progressRepository.GetByIterationIdAsync(iterationId);
        var progressList = progressRecords.OrderBy(p => p.RecordDate).ToList();

        var dataPoints = new List<BurndownPointDto>();
        
        // Calcular días de la iteración
        var totalDays = (iteration.EndDate - iteration.StartDate).Days;
        var initialTasks = progressList.FirstOrDefault()?.TotalTasks ?? 0;

        foreach (var progress in progressList)
        {
            var daysPassed = (progress.RecordDate - iteration.StartDate).Days;
            var remainingTasks = progress.TotalTasks - progress.CompletedTasks;
            var idealRemaining = initialTasks > 0 && totalDays > 0 
                ? initialTasks - (initialTasks * daysPassed / totalDays) 
                : 0;

            dataPoints.Add(new BurndownPointDto(
                progress.RecordDate,
                remainingTasks,
                idealRemaining,
                progress.CompletedTasks
            ));
        }

        return new BurndownDataDto(
            iteration.Id,
            iteration.Name,
            iteration.StartDate,
            iteration.EndDate,
            dataPoints
        );
    }

    private static string GetPhaseName(string phaseCode)
    {
        return phaseCode switch
        {
            "INCEPTION" => "Inicio",
            "ELABORATION" => "Elaboración",
            "CONSTRUCTION" => "Construcción",
            "TRANSITION" => "Transición",
            _ => phaseCode
        };
    }

    private static IterationProgressDto MapToDto(IterationProgress progress)
    {
        return new IterationProgressDto(
            progress.Id,
            progress.IterationId,
            progress.RecordDate,
            progress.CompletionPercentage,
            progress.TotalTasks,
            progress.CompletedTasks,
            progress.InProgressTasks,
            progress.BlockedTasks,
            progress.Blockers,
            progress.Observations,
            progress.CreatedAt
        );
    }
}
