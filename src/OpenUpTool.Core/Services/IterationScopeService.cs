using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Core.Services;

public class IterationScopeService : IIterationScopeService
{
    private readonly IIterationScopeRepository _repository;
    private readonly IUserStoryRepository _userStoryRepository;
    private readonly IArtifactRepository _artifactRepository;

    public IterationScopeService(
        IIterationScopeRepository repository,
        IUserStoryRepository userStoryRepository,
        IArtifactRepository artifactRepository)
    {
        _repository = repository;
        _userStoryRepository = userStoryRepository;
        _artifactRepository = artifactRepository;
    }

    public async Task<IEnumerable<IterationScopeDto>> GetByIterationIdAsync(Guid iterationId)
    {
        var scopes = await _repository.GetByIterationIdAsync(iterationId);
        var dtos = new List<IterationScopeDto>();

        foreach (var scope in scopes)
        {
            string? itemTitle = null;

            if (scope.ItemType == "story")
            {
                var story = await _userStoryRepository.GetByIdAsync(scope.ItemId);
                itemTitle = story?.Title;
            }
            else if (scope.ItemType == "artifact")
            {
                var artifact = await _artifactRepository.GetByIdAsync(scope.ItemId);
                itemTitle = artifact?.Title;
            }

            dtos.Add(new IterationScopeDto(
                scope.Id,
                scope.IterationId,
                scope.ItemType,
                scope.ItemId,
                itemTitle,
                scope.Description,
                scope.EstimatedHours,
                scope.Status,
                scope.AssignedTo,
                scope.AssignedUser != null ? $"{scope.AssignedUser.FirstName} {scope.AssignedUser.LastName}" : null,
                scope.CreatedAt
            ));
        }

        return dtos;
    }

    public async Task<IterationScopeDto?> GetByIdAsync(Guid id)
    {
        var scope = await _repository.GetByIdAsync(id);
        if (scope == null) return null;

        string? itemTitle = null;
        if (scope.ItemType == "story")
        {
            var story = await _userStoryRepository.GetByIdAsync(scope.ItemId);
            itemTitle = story?.Title;
        }
        else if (scope.ItemType == "artifact")
        {
            var artifact = await _artifactRepository.GetByIdAsync(scope.ItemId);
            itemTitle = artifact?.Title;
        }

        return new IterationScopeDto(
            scope.Id,
            scope.IterationId,
            scope.ItemType,
            scope.ItemId,
            itemTitle,
            scope.Description,
            scope.EstimatedHours,
            scope.Status,
            scope.AssignedTo,
            scope.AssignedUser != null ? $"{scope.AssignedUser.FirstName} {scope.AssignedUser.LastName}" : null,
            scope.CreatedAt
        );
    }

    public async Task<IterationScopeDto> AddToScopeAsync(AddToScopeDto dto)
    {
        // Verificar que no existe ya
        if (await _repository.ExistsAsync(dto.IterationId, dto.ItemType, dto.ItemId))
        {
            throw new InvalidOperationException("Este item ya está en el alcance de la iteración");
        }

        var scope = new IterationScope
        {
            Id = Guid.NewGuid(),
            IterationId = dto.IterationId,
            ItemType = dto.ItemType,
            ItemId = dto.ItemId,
            Description = dto.Description,
            EstimatedHours = dto.EstimatedHours,
            AssignedTo = dto.AssignedTo,
            Status = "pending"
        };

        var created = await _repository.CreateAsync(scope);
        
        string? itemTitle = null;
        if (scope.ItemType == "story")
        {
            var story = await _userStoryRepository.GetByIdAsync(scope.ItemId);
            itemTitle = story?.Title;
        }
        else if (scope.ItemType == "artifact")
        {
            var artifact = await _artifactRepository.GetByIdAsync(scope.ItemId);
            itemTitle = artifact?.Title;
        }

        return new IterationScopeDto(
            created.Id,
            created.IterationId,
            created.ItemType,
            created.ItemId,
            itemTitle,
            created.Description,
            created.EstimatedHours,
            created.Status,
            created.AssignedTo,
            null,
            created.CreatedAt
        );
    }

    public async Task<IterationScopeDto?> UpdateAsync(Guid id, UpdateScopeItemDto dto)
    {
        var scope = await _repository.GetByIdAsync(id);
        if (scope == null) return null;

        if (dto.Description != null) scope.Description = dto.Description;
        if (dto.EstimatedHours.HasValue) scope.EstimatedHours = dto.EstimatedHours;
        if (dto.Status != null) scope.Status = dto.Status;
        if (dto.AssignedTo.HasValue) scope.AssignedTo = dto.AssignedTo;

        var updated = await _repository.UpdateAsync(scope);

        string? itemTitle = null;
        if (scope.ItemType == "story")
        {
            var story = await _userStoryRepository.GetByIdAsync(scope.ItemId);
            itemTitle = story?.Title;
        }
        else if (scope.ItemType == "artifact")
        {
            var artifact = await _artifactRepository.GetByIdAsync(scope.ItemId);
            itemTitle = artifact?.Title;
        }

        return new IterationScopeDto(
            updated.Id,
            updated.IterationId,
            updated.ItemType,
            updated.ItemId,
            itemTitle,
            updated.Description,
            updated.EstimatedHours,
            updated.Status,
            updated.AssignedTo,
            updated.AssignedUser != null ? $"{updated.AssignedUser.FirstName} {updated.AssignedUser.LastName}" : null,
            updated.CreatedAt
        );
    }

    public async Task<bool> RemoveFromScopeAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }
}
