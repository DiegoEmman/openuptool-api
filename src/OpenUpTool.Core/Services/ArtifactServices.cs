using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Core.Services;

public class ArtifactService : IArtifactService
{
    private readonly IArtifactRepository _artifactRepository;
    private readonly IArtifactTypeRepository _artifactTypeRepository;

    public ArtifactService(IArtifactRepository artifactRepository, IArtifactTypeRepository artifactTypeRepository)
    {
        _artifactRepository = artifactRepository;
        _artifactTypeRepository = artifactTypeRepository;
    }

    public async Task<IEnumerable<ArtifactDto>> GetArtifactsByProjectAndPhaseAsync(Guid projectId, string phaseId)
    {
        var artifacts = await _artifactRepository.GetByProjectAndPhaseAsync(projectId, phaseId);
        return artifacts.Select(MapToDto);
    }

    public async Task<ArtifactDto> CreateArtifactAsync(CreateArtifactDto dto)
    {
        var artifactType = await _artifactTypeRepository.GetByIdAsync(dto.ArtifactTypeId);
        if (artifactType == null)
        {
            throw new InvalidOperationException("Tipo de artefacto no encontrado");
        }

        if (artifactType.Phase != dto.PhaseId)
        {
            throw new InvalidOperationException("El tipo de artefacto no pertenece a la fase especificada");
        }

        var artifact = new Artifact
        {
            Id = Guid.NewGuid(),
            ProjectId = dto.ProjectId,
            PhaseId = dto.PhaseId,
            ArtifactTypeId = dto.ArtifactTypeId,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Author = dto.Author?.Trim(),
            Status = "Pendiente",
            IsMandatory = artifactType.IsMandatory,
            ContentText = artifactType.DefaultFormat == "TEXT" ? "" : null
        };

        var created = await _artifactRepository.CreateAsync(artifact);
        created.ArtifactType = artifactType;
        return MapToDto(created);
    }

    public async Task<ArtifactDto?> UpdateArtifactAsync(Guid id, UpdateArtifactDto dto)
    {
        var artifact = await _artifactRepository.GetByIdAsync(id);
        if (artifact == null) return null;

        if (dto.Title != null) artifact.Title = dto.Title.Trim();
        if (dto.Description != null) artifact.Description = dto.Description.Trim();
        if (dto.Author != null) artifact.Author = dto.Author.Trim();
        if (dto.Status != null) artifact.Status = dto.Status;
        if (dto.ContentText != null) artifact.ContentText = dto.ContentText;

        var updated = await _artifactRepository.UpdateAsync(artifact);
        return MapToDto(updated);
    }

    private static ArtifactDto MapToDto(Artifact artifact)
    {
        return new ArtifactDto(
            artifact.Id,
            artifact.ProjectId,
            artifact.PhaseId,
            artifact.ArtifactTypeId,
            artifact.Title,
            artifact.Description,
            artifact.Author,
            artifact.CreatedAt,
            artifact.Status,
            artifact.IsMandatory,
            artifact.ContentText
        );
    }
}

public class ArtifactTypeService : IArtifactTypeService
{
    private readonly IArtifactTypeRepository _artifactTypeRepository;

    public ArtifactTypeService(IArtifactTypeRepository artifactTypeRepository)
    {
        _artifactTypeRepository = artifactTypeRepository;
    }

    public async Task<IEnumerable<ArtifactTypeDto>> GetAllArtifactTypesAsync()
    {
        var types = await _artifactTypeRepository.GetAllAsync();
        return types.Select(MapToDto);
    }

    public async Task<IEnumerable<ArtifactTypeDto>> GetArtifactTypesByPhaseAsync(string phase)
    {
        var types = await _artifactTypeRepository.GetByPhaseAsync(phase);
        return types.Select(MapToDto);
    }

    public async Task SeedDefaultInceptionTypesAsync()
    {
        var existing = await _artifactTypeRepository.GetByPhaseAsync("INCEPTION");
        if (existing.Any()) return;

        var inceptionTypes = new[]
        {
            new ArtifactType
            {
                Id = Guid.NewGuid(),
                Phase = "INCEPTION",
                Code = "VISION_DOC",
                Name = "Documento de Visión",
                Description = "Define la visión del producto.",
                IsMandatory = true,
                DefaultFormat = "TEXT"
            },
            new ArtifactType
            {
                Id = Guid.NewGuid(),
                Phase = "INCEPTION",
                Code = "STAKEHOLDERS",
                Name = "Lista de Stakeholders",
                Description = "Identifica actores clave.",
                IsMandatory = true,
                DefaultFormat = "TEXT"
            },
            new ArtifactType
            {
                Id = Guid.NewGuid(),
                Phase = "INCEPTION",
                Code = "INITIAL_RISKS",
                Name = "Lista de Riesgos Iniciales",
                Description = "Riesgos tempranos.",
                IsMandatory = true,
                DefaultFormat = "TEXT"
            },
            new ArtifactType
            {
                Id = Guid.NewGuid(),
                Phase = "INCEPTION",
                Code = "PROJECT_PLAN_V1",
                Name = "Plan de Proyecto (v1)",
                Description = "Versión inicial del plan.",
                IsMandatory = true,
                DefaultFormat = "TEXT"
            },
            new ArtifactType
            {
                Id = Guid.NewGuid(),
                Phase = "INCEPTION",
                Code = "HL_USE_CASES",
                Name = "Modelo Casos de Uso Alto Nivel",
                Description = "Casos de uso principales.",
                IsMandatory = false,
                DefaultFormat = "TEXT"
            }
        };

        foreach (var type in inceptionTypes)
        {
            await _artifactTypeRepository.CreateAsync(type);
        }
    }

    private static ArtifactTypeDto MapToDto(ArtifactType type)
    {
        return new ArtifactTypeDto(
            type.Id,
            type.Phase,
            type.Code,
            type.Name,
            type.Description,
            type.IsMandatory,
            type.DefaultFormat
        );
    }
}
