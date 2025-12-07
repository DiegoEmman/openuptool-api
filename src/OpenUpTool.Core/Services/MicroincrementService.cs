using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Core.Services;

public class MicroincrementService : IMicroincrementService
{
    private readonly IMicroincrementRepository _microincrementRepository;
    private readonly IArtifactRepository _artifactRepository;
    private readonly IIterationRepository _iterationRepository;

    public MicroincrementService(
        IMicroincrementRepository microincrementRepository,
        IArtifactRepository artifactRepository,
        IIterationRepository iterationRepository)
    {
        _microincrementRepository = microincrementRepository;
        _artifactRepository = artifactRepository;
        _iterationRepository = iterationRepository;
    }

    public async Task<IEnumerable<MicroincrementDto>> GetAllAsync()
    {
        var microincrements = await _microincrementRepository.GetAllAsync();
        return microincrements.Select(MapToDto);
    }

    public async Task<IEnumerable<MicroincrementDto>> GetByIterationIdAsync(Guid iterationId)
    {
        var microincrements = await _microincrementRepository.GetByIterationIdAsync(iterationId);
        return microincrements.Select(MapToDto);
    }

    public async Task<IEnumerable<MicroincrementDto>> GetByArtifactIdAsync(Guid artifactId)
    {
        var microincrements = await _microincrementRepository.GetByArtifactIdAsync(artifactId);
        return microincrements.Select(MapToDto);
    }

    public async Task<IEnumerable<MicroincrementDto>> GetByAuthorAsync(string author)
    {
        var microincrements = await _microincrementRepository.GetByAuthorAsync(author);
        return microincrements.Select(MapToDto);
    }

    public async Task<IEnumerable<MicroincrementDto>> GetByTypeAsync(string type)
    {
        var microincrements = await _microincrementRepository.GetByTypeAsync(type);
        return microincrements.Select(MapToDto);
    }

    public async Task<IEnumerable<MicroincrementDto>> GetFilteredAsync(
        Guid? iterationId, 
        Guid? artifactId, 
        string? author,
        string? type)
    {
        var microincrements = await _microincrementRepository.GetFilteredAsync(iterationId, artifactId, author, type);
        return microincrements.Select(MapToDto);
    }

    public async Task<MicroincrementDto?> GetByIdAsync(Guid id)
    {
        var microincrement = await _microincrementRepository.GetByIdAsync(id);
        return microincrement == null ? null : MapToDto(microincrement);
    }

    public async Task<MicroincrementDto> CreateAsync(CreateMicroincrementDto dto)
    {
        // Validar tipo
        if (!string.IsNullOrEmpty(dto.Type) && dto.Type != "tecnico" && dto.Type != "funcional")
        {
            throw new ArgumentException("El tipo debe ser 'tecnico' o 'funcional'");
        }

        // Validar que el artifact existe
        var artifact = await _artifactRepository.GetByIdAsync(dto.ArtifactId);
        if (artifact == null)
        {
            throw new ArgumentException($"Artifact con ID {dto.ArtifactId} no existe");
        }

        // Validar iteration si está especificada
        if (dto.IterationId.HasValue)
        {
            var iteration = await _iterationRepository.GetByIdAsync(dto.IterationId.Value);
            if (iteration == null)
            {
                throw new ArgumentException($"Iteration con ID {dto.IterationId} no existe");
            }
        }

        var microincrement = new Microincrement
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Date = dto.Date,
            Author = dto.Author,
            Type = dto.Type ?? "funcional",
            EvidenceUrl = dto.EvidenceUrl,
            EvidenceFilePath = dto.EvidenceFilePath,
            IterationId = dto.IterationId,
            ArtifactId = dto.ArtifactId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _microincrementRepository.CreateAsync(microincrement);
        
        // Recargar con relaciones
        var result = await _microincrementRepository.GetByIdAsync(created.Id);
        return MapToDto(result!);
    }

    public async Task<MicroincrementDto?> UpdateAsync(Guid id, UpdateMicroincrementDto dto)
    {
        var microincrement = await _microincrementRepository.GetByIdAsync(id);
        if (microincrement == null)
        {
            return null;
        }

        // Validar tipo si se está actualizando
        if (dto.Type != null && dto.Type != "tecnico" && dto.Type != "funcional")
        {
            throw new ArgumentException("El tipo debe ser 'tecnico' o 'funcional'");
        }

        // Validar artifact si se está actualizando
        if (dto.ArtifactId.HasValue)
        {
            var artifact = await _artifactRepository.GetByIdAsync(dto.ArtifactId.Value);
            if (artifact == null)
            {
                throw new ArgumentException($"Artifact con ID {dto.ArtifactId} no existe");
            }
            microincrement.ArtifactId = dto.ArtifactId.Value;
        }

        // Validar iteration si se está actualizando
        if (dto.IterationId.HasValue)
        {
            var iteration = await _iterationRepository.GetByIdAsync(dto.IterationId.Value);
            if (iteration == null)
            {
                throw new ArgumentException($"Iteration con ID {dto.IterationId} no existe");
            }
            microincrement.IterationId = dto.IterationId.Value;
        }

        if (dto.Title != null) microincrement.Title = dto.Title;
        if (dto.Description != null) microincrement.Description = dto.Description;
        if (dto.Date.HasValue) microincrement.Date = dto.Date.Value;
        if (dto.Author != null) microincrement.Author = dto.Author;
        if (dto.Type != null) microincrement.Type = dto.Type;
        if (dto.EvidenceUrl != null) microincrement.EvidenceUrl = dto.EvidenceUrl;
        if (dto.EvidenceFilePath != null) microincrement.EvidenceFilePath = dto.EvidenceFilePath;
        
        microincrement.UpdatedAt = DateTime.UtcNow;

        var updated = await _microincrementRepository.UpdateAsync(microincrement);
        
        // Recargar con relaciones
        var result = await _microincrementRepository.GetByIdAsync(updated.Id);
        return MapToDto(result!);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var microincrement = await _microincrementRepository.GetByIdAsync(id);
        if (microincrement == null)
        {
            return false;
        }

        await _microincrementRepository.DeleteAsync(id);
        return true;
    }

    private MicroincrementDto MapToDto(Microincrement microincrement)
    {
        return new MicroincrementDto
        {
            Id = microincrement.Id,
            Title = microincrement.Title,
            Description = microincrement.Description,
            Date = microincrement.Date,
            Author = microincrement.Author,
            Type = microincrement.Type,
            EvidenceUrl = microincrement.EvidenceUrl,
            EvidenceFilePath = microincrement.EvidenceFilePath,
            IterationId = microincrement.IterationId,
            IterationName = microincrement.Iteration?.Name,
            ArtifactId = microincrement.ArtifactId,
            ArtifactTitle = microincrement.Artifact?.Title ?? string.Empty,
            CreatedAt = microincrement.CreatedAt,
            UpdatedAt = microincrement.UpdatedAt
        };
    }
}
