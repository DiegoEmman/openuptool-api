using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Core.Services;

public class UserStoryService : IUserStoryService
{
    private readonly IUserStoryRepository _repository;

    public UserStoryService(IUserStoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<UserStoryDto>> GetByProjectIdAsync(Guid projectId)
    {
        var stories = await _repository.GetByProjectIdAsync(projectId);
        return stories.Select(MapToDto);
    }

    public async Task<UserStoryDto?> GetByIdAsync(Guid id)
    {
        var story = await _repository.GetByIdAsync(id);
        return story == null ? null : MapToDto(story);
    }

    public async Task<UserStoryDto> CreateAsync(CreateUserStoryDto dto)
    {
        var story = new UserStory
        {
            Id = Guid.NewGuid(),
            ProjectId = dto.ProjectId,
            Title = dto.Title,
            Description = dto.Description,
            AcceptanceCriteria = dto.AcceptanceCriteria,
            Priority = dto.Priority,
            StoryPoints = dto.StoryPoints,
            Status = "backlog"
        };

        var created = await _repository.CreateAsync(story);
        return MapToDto(created);
    }

    public async Task<UserStoryDto?> UpdateAsync(Guid id, UpdateUserStoryDto dto)
    {
        var story = await _repository.GetByIdAsync(id);
        if (story == null) return null;

        if (dto.Title != null) story.Title = dto.Title;
        if (dto.Description != null) story.Description = dto.Description;
        if (dto.AcceptanceCriteria != null) story.AcceptanceCriteria = dto.AcceptanceCriteria;
        if (dto.Priority != null) story.Priority = dto.Priority;
        if (dto.StoryPoints.HasValue) story.StoryPoints = dto.StoryPoints;
        if (dto.Status != null) story.Status = dto.Status;

        var updated = await _repository.UpdateAsync(story);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<UserStoryDto>> GetBacklogAsync(Guid projectId)
    {
        var stories = await _repository.GetByStatusAsync(projectId, "backlog");
        return stories.Select(MapToDto);
    }

    private static UserStoryDto MapToDto(UserStory story)
    {
        return new UserStoryDto(
            story.Id,
            story.ProjectId,
            story.Title,
            story.Description,
            story.AcceptanceCriteria,
            story.Priority,
            story.StoryPoints,
            story.Status,
            story.CreatedBy,
            story.CreatedAt
        );
    }
}
