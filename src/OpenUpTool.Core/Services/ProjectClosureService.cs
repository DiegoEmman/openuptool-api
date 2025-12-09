using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using System.Text.Json;

namespace OpenUpTool.Core.Services;

public class ProjectClosureService : IProjectClosureService
{
    private readonly IProjectClosureRepository _closureRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IArtifactRepository _artifactRepository;
    private readonly IArtifactTypeRepository _artifactTypeRepository;

    public ProjectClosureService(
        IProjectClosureRepository closureRepository,
        IProjectRepository projectRepository,
        IArtifactRepository artifactRepository,
        IArtifactTypeRepository artifactTypeRepository)
    {
        _closureRepository = closureRepository;
        _projectRepository = projectRepository;
        _artifactRepository = artifactRepository;
        _artifactTypeRepository = artifactTypeRepository;
    }

    public async Task<IEnumerable<ProjectClosureDto>> GetAllClosuresAsync()
    {
        var closures = await _closureRepository.GetAllAsync();
        return closures.Select(MapToDto);
    }

    public async Task<ProjectClosureDto?> GetClosureByIdAsync(Guid id)
    {
        var closure = await _closureRepository.GetByIdAsync(id);
        return closure == null ? null : MapToDto(closure);
    }

    public async Task<ProjectClosureDto?> GetClosureByProjectIdAsync(Guid projectId)
    {
        var closure = await _closureRepository.GetByProjectIdAsync(projectId);
        return closure == null ? null : MapToDto(closure);
    }

    public async Task<ProjectClosureDto> CreateClosureAsync(CreateProjectClosureDto dto, string closedBy)
    {
        // Validar que el proyecto existe
        var project = await _projectRepository.GetByIdAsync(dto.ProjectId);
        if (project == null)
            throw new ArgumentException($"Proyecto con ID {dto.ProjectId} no encontrado");

        // Validar que no exista ya un cierre para este proyecto
        var existingClosure = await _closureRepository.GetByProjectIdAsync(dto.ProjectId);
        if (existingClosure != null)
            throw new InvalidOperationException($"El proyecto ya tiene un documento de cierre");

        // Calcular métricas del checklist
        var mandatoryCount = dto.Checklist.Count(c => c.IsMandatory);
        var completedCount = dto.Checklist.Count(c => c.IsCompleted);
        var completedMandatoryCount = dto.Checklist.Count(c => c.IsMandatory && c.IsCompleted);
        var allMandatoryMet = mandatoryCount > 0 && completedMandatoryCount == mandatoryCount;

        var closure = new ProjectClosure
        {
            Id = Guid.NewGuid(),
            ProjectId = dto.ProjectId,
            ClosedBy = closedBy,
            ClosureDate = DateTime.UtcNow,
            Summary = dto.Summary,
            LessonsLearned = dto.LessonsLearned,
            Recommendations = dto.Recommendations,
            ChecklistData = JsonSerializer.Serialize(dto.Checklist),
            TotalCriteria = dto.Checklist.Count,
            CompletedCriteria = completedCount,
            MandatoryCriteria = mandatoryCount,
            CompletedMandatoryCriteria = completedMandatoryCount,
            AllMandatoryCriteriaMet = allMandatoryMet,
            Status = allMandatoryMet ? "PendingApproval" : "Draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _closureRepository.CreateAsync(closure);
        created.Project = project;
        return MapToDto(created);
    }

    public async Task<ProjectClosureDto?> UpdateClosureAsync(Guid id, UpdateProjectClosureDto dto)
    {
        var closure = await _closureRepository.GetByIdAsync(id);
        if (closure == null) return null;

        // No permitir actualización si ya está aprobado
        if (closure.Status == "Approved")
            throw new InvalidOperationException("No se puede actualizar un cierre ya aprobado");

        if (dto.Summary != null) closure.Summary = dto.Summary;
        if (dto.LessonsLearned != null) closure.LessonsLearned = dto.LessonsLearned;
        if (dto.Recommendations != null) closure.Recommendations = dto.Recommendations;

        if (dto.Checklist != null)
        {
            closure.ChecklistData = JsonSerializer.Serialize(dto.Checklist);
            
            var mandatoryCount = dto.Checklist.Count(c => c.IsMandatory);
            var completedCount = dto.Checklist.Count(c => c.IsCompleted);
            var completedMandatoryCount = dto.Checklist.Count(c => c.IsMandatory && c.IsCompleted);
            
            closure.TotalCriteria = dto.Checklist.Count;
            closure.CompletedCriteria = completedCount;
            closure.MandatoryCriteria = mandatoryCount;
            closure.CompletedMandatoryCriteria = completedMandatoryCount;
            closure.AllMandatoryCriteriaMet = mandatoryCount > 0 && completedMandatoryCount == mandatoryCount;
            
            // Actualizar estado si ahora cumple todos los obligatorios
            if (closure.AllMandatoryCriteriaMet && closure.Status == "Draft")
            {
                closure.Status = "PendingApproval";
            }
        }

        closure.UpdatedAt = DateTime.UtcNow;
        var updated = await _closureRepository.UpdateAsync(closure);
        return MapToDto(updated);
    }

    public async Task<ProjectClosureDto?> ApproveClosureAsync(Guid id, string approvedBy, ApproveClosureDto dto)
    {
        var closure = await _closureRepository.GetByIdAsync(id);
        if (closure == null) return null;

        if (!closure.AllMandatoryCriteriaMet)
            throw new InvalidOperationException("No se puede aprobar el cierre: faltan criterios obligatorios por cumplir");

        if (dto.Approve)
        {
            closure.Status = "Approved";
            closure.ApprovedAt = DateTime.UtcNow;
            closure.ApprovedBy = approvedBy;
            closure.RejectionReason = null;
        }
        else
        {
            closure.Status = "Rejected";
            closure.RejectionReason = dto.RejectionReason;
            closure.ApprovedAt = null;
            closure.ApprovedBy = null;
        }

        closure.UpdatedAt = DateTime.UtcNow;
        var updated = await _closureRepository.UpdateAsync(closure);
        return MapToDto(updated);
    }

    public async Task<ClosureValidationDto> ValidateClosureAsync(Guid projectId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new ArgumentException($"Proyecto con ID {projectId} no encontrado");

        var checklist = new List<ClosureCriteriaDto>();

        // 1) Add phase completion items
        var phases = project.Phases?.ToList() ?? new List<Phase>();
        foreach (var phase in phases)
        {
            checklist.Add(new ClosureCriteriaDto
            {
                CriteriaId = $"phase-{phase.PhaseCode}",
                Name = $"Fase: {phase.Name}",
                IsMandatory = true,
                IsCompleted = string.Equals(phase.Status, "Completada", StringComparison.OrdinalIgnoreCase),
                Notes = $"Estado: {phase.Status}"
            });
        }

        // 2) Add artifact-type based items for mandatory ArtifactTypes
        try
        {
            var artifactTypes = (await _artifactTypeRepository.GetAllAsync()).ToList();
            foreach (var at in artifactTypes.Where(a => a.IsMandatory))
            {
                var artifacts = (await _artifactRepository.GetByProjectAndPhaseAsync(projectId, at.Phase)).ToList();
                var matching = artifacts.Where(a => a.ArtifactTypeId == at.Id).ToList();

                var criteria = new ClosureCriteriaDto
                {
                    CriteriaId = at.Id.ToString(),
                    Name = at.Name,
                    IsMandatory = true,
                    IsCompleted = false,
                    Notes = string.Empty
                };

                if (!matching.Any())
                {
                    criteria.IsCompleted = false;
                    criteria.Notes = $"No se encontró ningún artefacto del tipo {at.Name} en la fase {at.Phase}";
                }
                else
                {
                    // Check versions
                    var deliveredInfos = new List<string>();
                    foreach (var art in matching)
                    {
                        if (art.Versions != null && art.Versions.Any())
                        {
                            var latest = art.Versions.OrderByDescending(v => v.VersionNumber).First();
                            deliveredInfos.Add($"artifact:{art.Id} version:{latest.Id} file:{latest.FileName}");
                        }
                    }

                    if (deliveredInfos.Any())
                    {
                        criteria.IsCompleted = true;
                        criteria.Notes = string.Join("; ", deliveredInfos);
                    }
                    else
                    {
                        criteria.IsCompleted = false;
                        criteria.Notes = $"Se encontraron artefactos del tipo {at.Name} pero sin versiones entregadas";
                    }
                }

                checklist.Add(criteria);
            }
        }
        catch (Exception ex)
        {
            checklist.Add(new ClosureCriteriaDto
            {
                CriteriaId = "artifact-validation-error",
                Name = "Validación de artefactos",
                IsMandatory = true,
                IsCompleted = false,
                Notes = $"Error al validar artefactos: {ex.Message}"
            });
        }

        var missing = checklist.Where(c => c.IsMandatory && !c.IsCompleted).Select(c => c.Name).ToList();
        var totalMandatory = checklist.Count(c => c.IsMandatory);
        var completedMandatory = checklist.Count(c => c.IsMandatory && c.IsCompleted);

        return new ClosureValidationDto
        {
            CanClose = !missing.Any(),
            MissingMandatoryCriteria = missing,
            TotalMandatory = totalMandatory,
            CompletedMandatory = completedMandatory,
            ChecklistPreview = checklist
        };
    }

    public async Task<bool> DeleteClosureAsync(Guid id)
    {
        var closure = await _closureRepository.GetByIdAsync(id);
        if (closure == null) return false;

        if (closure.Status == "Approved")
            throw new InvalidOperationException("No se puede eliminar un cierre aprobado");

        await _closureRepository.DeleteAsync(id);
        return true;
    }

    private static ProjectClosureDto MapToDto(ProjectClosure closure)
    {
        var checklist = new List<ClosureCriteriaDto>();
        try
        {
            checklist = JsonSerializer.Deserialize<List<ClosureCriteriaDto>>(closure.ChecklistData) ?? new();
        }
        catch { }

        return new ProjectClosureDto
        {
            Id = closure.Id,
            ProjectId = closure.ProjectId,
            ProjectName = closure.Project?.Name ?? string.Empty,
            ClosedBy = closure.ClosedBy,
            ClosureDate = closure.ClosureDate,
            Summary = closure.Summary,
            LessonsLearned = closure.LessonsLearned,
            Recommendations = closure.Recommendations,
            Checklist = checklist,
            AllMandatoryCriteriaMet = closure.AllMandatoryCriteriaMet,
            TotalCriteria = closure.TotalCriteria,
            CompletedCriteria = closure.CompletedCriteria,
            MandatoryCriteria = closure.MandatoryCriteria,
            CompletedMandatoryCriteria = closure.CompletedMandatoryCriteria,
            Status = closure.Status,
            RejectionReason = closure.RejectionReason,
            ApprovedAt = closure.ApprovedAt,
            ApprovedBy = closure.ApprovedBy,
            CreatedAt = closure.CreatedAt,
            UpdatedAt = closure.UpdatedAt
        };
    }
}
