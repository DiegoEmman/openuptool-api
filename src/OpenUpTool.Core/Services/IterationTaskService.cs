using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Core.Services;

public class IterationTaskService : IIterationTaskService
{
    private readonly IIterationTaskRepository _taskRepository;
    private readonly IIterationRepository _iterationRepository;

    public IterationTaskService(IIterationTaskRepository taskRepository, IIterationRepository iterationRepository)
    {
        _taskRepository = taskRepository;
        _iterationRepository = iterationRepository;
    }

    public async Task<IEnumerable<IterationTaskDto>> GetTasksByIterationAsync(Guid iterationId)
    {
        var tasks = await _taskRepository.GetByIterationIdAsync(iterationId);
        return tasks.Select(MapToDto);
    }

    public async Task<IterationTaskDto> CreateTaskAsync(Guid iterationId, CreateIterationTaskDto dto)
    {
        var iteration = await _iterationRepository.GetByIdAsync(iterationId);
        if (iteration == null)
            throw new InvalidOperationException("Iteración no encontrada");

        var task = new IterationTask
        {
            Id = Guid.NewGuid(),
            IterationId = iterationId,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Status = "pending",
            EstimatedHours = dto.EstimatedHours,
            AssignedTo = dto.AssignedTo,
            StartDate = dto.StartDate.HasValue ? DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Utc) : null,
            EndDate = dto.EndDate.HasValue ? DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc) : null,
            Priority = dto.Priority
        };

        var created = await _taskRepository.CreateAsync(task);
        var result = await _taskRepository.GetByIdAsync(created.Id);
        return MapToDto(result!);
    }

    public async Task<IterationTaskDto?> UpdateTaskAsync(Guid id, UpdateIterationTaskDto dto)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
            task.Name = dto.Name.Trim();

        if (dto.Description != null)
            task.Description = dto.Description.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Status))
            task.Status = dto.Status;

        if (dto.EstimatedHours.HasValue)
            task.EstimatedHours = dto.EstimatedHours.Value;

        if (dto.ActualHours.HasValue)
            task.ActualHours = dto.ActualHours.Value;

        if (dto.AssignedTo.HasValue)
            task.AssignedTo = dto.AssignedTo.Value;

        if (dto.StartDate.HasValue)
            task.StartDate = DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Utc);

        if (dto.EndDate.HasValue)
            task.EndDate = DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc);

        if (dto.Priority.HasValue)
            task.Priority = dto.Priority.Value;

        if (dto.BlockerDescription != null)
            task.BlockerDescription = dto.BlockerDescription.Trim();

        var updated = await _taskRepository.UpdateAsync(task);
        var result = await _taskRepository.GetByIdAsync(updated.Id);
        return MapToDto(result!);
    }

    public async Task<bool> DeleteTaskAsync(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
            return false;

        await _taskRepository.DeleteAsync(id);
        return true;
    }

    private static IterationTaskDto MapToDto(IterationTask task)
    {
        var assignedToName = task.AssignedUser != null 
            ? $"{task.AssignedUser.FirstName} {task.AssignedUser.LastName}".Trim()
            : null;

        return new IterationTaskDto(
            task.Id,
            task.IterationId,
            task.Name,
            task.Description,
            task.Status,
            task.EstimatedHours,
            task.ActualHours,
            task.AssignedTo,
            assignedToName,
            task.StartDate,
            task.EndDate,
            task.Priority,
            task.BlockerDescription,
            task.CreatedAt
        );
    }
}
