using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using System.Text.Json;

namespace OpenUpTool.Core.Services;

public class ArtifactService : IArtifactService
{
    private readonly IArtifactRepository _artifactRepository;
    private readonly IArtifactTypeRepository _artifactTypeRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IArtifactMovementHistoryRepository _movementHistoryRepository;

    // Orden válido de fases OpenUP
    private static readonly string[] ValidPhaseOrder = { "INCEPTION", "ELABORATION", "CONSTRUCTION", "TRANSITION" };

    public ArtifactService(
        IArtifactRepository artifactRepository, 
        IArtifactTypeRepository artifactTypeRepository,
        IFileStorageService fileStorageService,
        IArtifactMovementHistoryRepository movementHistoryRepository)
    {
        _artifactRepository = artifactRepository;
        _artifactTypeRepository = artifactTypeRepository;
        _fileStorageService = fileStorageService;
        _movementHistoryRepository = movementHistoryRepository;
    }

    public async Task<IEnumerable<ArtifactDto>> GetArtifactsByProjectAndPhaseAsync(Guid projectId, string phaseId)
    {
        var artifacts = await _artifactRepository.GetByProjectAndPhaseAsync(projectId, phaseId);
        return artifacts.Select(MapToDto);
    }

    public async Task<ArtifactDto> CreateArtifactAsync(CreateArtifactDto dto, Stream? fileStream = null, string? fileName = null)
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
            IsMandatory = dto.IsMandatory,
            ContentText = dto.ContentText,
            FileCategory = dto.FileCategory,
            RepositoryUrl = dto.RepositoryUrl?.Trim(),
            RepositoryVersion = dto.RepositoryVersion?.Trim(),
            BuildNumber = dto.BuildNumber?.Trim()
        };

        // Si hay archivo adjunto, guardarlo
        if (fileStream != null && fileName != null && dto.FileCategory != null)
        {
            if (!_fileStorageService.IsValidFileFormat(fileName, dto.FileCategory))
            {
                throw new InvalidOperationException($"Formato de archivo no permitido para categoría {dto.FileCategory}");
            }

            var (filePath, savedFileName, fileSize) = await _fileStorageService.SaveFileAsync(
                dto.ProjectId, artifact.Id, fileStream, fileName, dto.FileCategory);

            artifact.FilePath = filePath;
            artifact.FileName = savedFileName;
            artifact.FileSize = fileSize;
            artifact.MimeType = GetMimeType(fileName);
        }

        var created = await _artifactRepository.CreateAsync(artifact);
        created.ArtifactType = artifactType;
        return MapToDto(created);
    }

    public async Task<ArtifactDto?> UpdateArtifactAsync(Guid id, UpdateArtifactDto dto, Stream? fileStream = null, string? fileName = null)
    {
        var artifact = await _artifactRepository.GetByIdAsync(id);
        if (artifact == null) return null;

        if (dto.Title != null) artifact.Title = dto.Title.Trim();
        if (dto.Description != null) artifact.Description = dto.Description.Trim();
        if (dto.Author != null) artifact.Author = dto.Author.Trim();
        if (dto.Status != null) artifact.Status = dto.Status;
        if (dto.IsMandatory.HasValue) artifact.IsMandatory = dto.IsMandatory.Value;
        if (dto.ContentText != null) artifact.ContentText = dto.ContentText;
        if (dto.FileCategory != null) artifact.FileCategory = dto.FileCategory;
        if (dto.RepositoryUrl != null) artifact.RepositoryUrl = dto.RepositoryUrl.Trim();
        if (dto.RepositoryVersion != null) artifact.RepositoryVersion = dto.RepositoryVersion.Trim();
        if (dto.BuildNumber != null) artifact.BuildNumber = dto.BuildNumber.Trim();

        // Si hay nuevo archivo, eliminar el anterior y guardar el nuevo
        if (fileStream != null && fileName != null && dto.FileCategory != null)
        {
            if (!_fileStorageService.IsValidFileFormat(fileName, dto.FileCategory))
            {
                throw new InvalidOperationException($"Formato de archivo no permitido para categoría {dto.FileCategory}");
            }

            // Eliminar archivo anterior si existe
            if (!string.IsNullOrEmpty(artifact.FilePath))
            {
                await _fileStorageService.DeleteFileAsync(artifact.FilePath);
            }

            var (filePath, savedFileName, fileSize) = await _fileStorageService.SaveFileAsync(
                artifact.ProjectId, artifact.Id, fileStream, fileName, dto.FileCategory);

            artifact.FilePath = filePath;
            artifact.FileName = savedFileName;
            artifact.FileSize = fileSize;
            artifact.MimeType = GetMimeType(fileName);
        }

        var updated = await _artifactRepository.UpdateAsync(artifact);
        return MapToDto(updated);
    }

    public async Task<Stream?> GetArtifactFileAsync(Guid artifactId)
    {
        var artifact = await _artifactRepository.GetByIdAsync(artifactId);
        if (artifact == null || string.IsNullOrEmpty(artifact.FilePath))
            return null;

        return await _fileStorageService.GetFileStreamAsync(artifact.FilePath);
    }

    public Task<List<AllowedFileFormatsDto>> GetAllowedFileFormatsAsync()
    {
        var formats = new List<AllowedFileFormatsDto>
        {
            new AllowedFileFormatsDto(
                "DIAGRAM",
                new List<string> { ".png", ".svg", ".pdf", ".jpg", ".jpeg" },
                new List<string> { "image/png", "image/svg+xml", "application/pdf", "image/jpeg" },
                10 * 1024 * 1024 // 10 MB
            ),
            new AllowedFileFormatsDto(
                "PROTOTYPE",
                new List<string> { ".png", ".jpg", ".jpeg", ".gif", ".pdf" },
                new List<string> { "image/png", "image/jpeg", "image/gif", "application/pdf" },
                15 * 1024 * 1024 // 15 MB
            ),
            new AllowedFileFormatsDto(
                "DOCUMENT",
                new List<string> { ".pdf", ".docx", ".doc", ".txt" },
                new List<string> { "application/pdf", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/msword", "text/plain" },
                20 * 1024 * 1024 // 20 MB
            )
        };

        return Task.FromResult(formats);
    }

    public async Task<ArtifactDto?> LinkRepositoryAsync(Guid artifactId, LinkRepositoryRequest request)
    {
        var artifact = await _artifactRepository.GetByIdAsync(artifactId);
        if (artifact == null) return null;

        // Validar URL
        if (!Uri.TryCreate(request.RepositoryUrl, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException("URL de repositorio inválida");
        }

        artifact.RepositoryUrl = request.RepositoryUrl.Trim();
        artifact.RepositoryVersion = request.RepositoryVersion?.Trim();
        artifact.BuildNumber = request.BuildNumber?.Trim();

        var updated = await _artifactRepository.UpdateAsync(artifact);
        return MapToDto(updated);
    }

    public async Task<ArtifactDto?> AddTestCaseAsync(Guid artifactId, AddTestCaseRequest request)
    {
        var artifact = await _artifactRepository.GetByIdAsync(artifactId);
        if (artifact == null) return null;

        var testData = string.IsNullOrEmpty(artifact.TestData)
            ? new TestDataJson { TestCases = new List<TestCaseDto>(), TestResults = new List<TestResultDto>() }
            : JsonSerializer.Deserialize<TestDataJson>(artifact.TestData) ?? new TestDataJson { TestCases = new List<TestCaseDto>(), TestResults = new List<TestResultDto>() };

        var newTestCase = new TestCaseDto(
            request.TestId,
            request.Title,
            request.Description,
            request.Steps,
            request.ExpectedResult,
            request.Priority,
            DateTime.UtcNow
        );

        testData.TestCases ??= new List<TestCaseDto>();
        testData.TestCases.Add(newTestCase);

        artifact.TestData = JsonSerializer.Serialize(testData);
        var updated = await _artifactRepository.UpdateAsync(artifact);
        return MapToDto(updated);
    }

    public async Task<ArtifactDto?> AddTestResultAsync(Guid artifactId, AddTestResultRequest request)
    {
        var artifact = await _artifactRepository.GetByIdAsync(artifactId);
        if (artifact == null) return null;

        var testData = string.IsNullOrEmpty(artifact.TestData)
            ? new TestDataJson { TestCases = new List<TestCaseDto>(), TestResults = new List<TestResultDto>() }
            : JsonSerializer.Deserialize<TestDataJson>(artifact.TestData) ?? new TestDataJson { TestCases = new List<TestCaseDto>(), TestResults = new List<TestResultDto>() };

        var newResult = new TestResultDto(
            request.TestCaseId,
            request.Result,
            request.ExecutedBy,
            DateTime.UtcNow,
            request.Notes,
            request.Defects
        );

        testData.TestResults ??= new List<TestResultDto>();
        testData.TestResults.Add(newResult);

        artifact.TestData = JsonSerializer.Serialize(testData);
        var updated = await _artifactRepository.UpdateAsync(artifact);
        return MapToDto(updated);
    }

    public async Task<ArtifactDto?> AddIterationActivityAsync(Guid artifactId, AddIterationActivityRequest request)
    {
        var artifact = await _artifactRepository.GetByIdAsync(artifactId);
        if (artifact == null) return null;

        var iterationData = string.IsNullOrEmpty(artifact.IterationData)
            ? new IterationDataJson { Activities = new List<IterationActivityDto>() }
            : JsonSerializer.Deserialize<IterationDataJson>(artifact.IterationData) ?? new IterationDataJson { Activities = new List<IterationActivityDto>() };

        var newActivity = new IterationActivityDto(
            Guid.NewGuid().ToString(),
            request.Type,
            request.Description,
            request.Participants,
            DateTime.UtcNow,
            request.Tags
        );

        iterationData.Activities ??= new List<IterationActivityDto>();
        iterationData.Activities.Add(newActivity);

        artifact.IterationData = JsonSerializer.Serialize(iterationData);
        var updated = await _artifactRepository.UpdateAsync(artifact);
        return MapToDto(updated);
    }

    private static string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    private static ArtifactDto MapToDto(Artifact artifact)
    {
        // Deserializar datos estructurados de JSON
        List<TestCaseDto>? testCases = null;
        List<TestResultDto>? testResults = null;
        List<IterationActivityDto>? iterationActivities = null;

        if (!string.IsNullOrEmpty(artifact.TestData))
        {
            try
            {
                var testData = JsonSerializer.Deserialize<TestDataJson>(artifact.TestData);
                testCases = testData?.TestCases;
                testResults = testData?.TestResults;
            }
            catch { /* Ignore deserialization errors */ }
        }

        if (!string.IsNullOrEmpty(artifact.IterationData))
        {
            try
            {
                var iterationData = JsonSerializer.Deserialize<IterationDataJson>(artifact.IterationData);
                iterationActivities = iterationData?.Activities;
            }
            catch { /* Ignore deserialization errors */ }
        }

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
            artifact.ContentText,
            artifact.FilePath,
            artifact.FileName,
            artifact.FileSize,
            artifact.MimeType,
            artifact.FileCategory,
            artifact.RepositoryUrl,
            artifact.RepositoryVersion,
            artifact.BuildNumber,
            testCases,
            testResults,
            iterationActivities
        );
    }

    public async Task<PhaseValidationDto> ValidatePhaseCompletionAsync(Guid projectId, string phaseId)
    {
        // Obtener todos los artefactos de la fase
        var artifacts = await _artifactRepository.GetByProjectAndPhaseAsync(projectId, phaseId);
        
        // Filtrar solo los obligatorios
        var mandatoryArtifacts = artifacts.Where(a => a.IsMandatory).ToList();
        
        // Lista para artefactos incompletos
        var missingArtifacts = new List<MissingArtifactDto>();

        foreach (var artifact in mandatoryArtifacts)
        {
            // Un artefacto obligatorio está completo si tiene al menos una versión
            var hasVersions = artifact.Versions != null && artifact.Versions.Any();
            
            if (!hasVersions)
            {
                missingArtifacts.Add(new MissingArtifactDto(
                    ArtifactId: artifact.Id,
                    Title: artifact.Title,
                    ArtifactType: artifact.ArtifactType?.Name ?? "Desconocido",
                    Status: artifact.Status,
                    HasVersions: false
                ));
            }
        }

        var canAdvance = missingArtifacts.Count == 0;
        var message = canAdvance 
            ? "Todos los artefactos obligatorios están completos. El proyecto puede avanzar a la siguiente fase."
            : $"Faltan {missingArtifacts.Count} artefacto(s) obligatorio(s) por completar.";

        return new PhaseValidationDto(
            CanAdvance: canAdvance,
            Phase: phaseId,
            TotalMandatoryArtifacts: mandatoryArtifacts.Count,
            CompletedMandatoryArtifacts: mandatoryArtifacts.Count - missingArtifacts.Count,
            MissingArtifacts: missingArtifacts,
            Message: message
        );
    }

    // Clases auxiliares para serialización JSON
    private class TestDataJson
    {
        public List<TestCaseDto>? TestCases { get; set; }
        public List<TestResultDto>? TestResults { get; set; }
    }

    private class IterationDataJson
    {
        public List<IterationActivityDto>? Activities { get; set; }
    }

    // HU-020: Métodos de reasignación de artefactos

    public async Task<ReassignmentResultDto> ReassignToPhaseAsync(Guid artifactId, string userId, ReassignArtifactDto dto)
    {
        var artifact = await _artifactRepository.GetByIdAsync(artifactId);
        if (artifact == null)
        {
            return new ReassignmentResultDto(
                Success: false, 
                Artifact: null,
                Movement: null, 
                HasViolations: false,
                Violations: new List<ReassignmentViolationDto>(),
                Message: "Artefacto no encontrado"
            );
        }

        var oldPhaseId = artifact.PhaseId;
        var violations = ValidatePhaseChange(oldPhaseId, dto.NewPhaseId);

        // Si hay violaciones y no se confirmó
        if (violations.Any() && !dto.ConfirmViolation)
        {
            return new ReassignmentResultDto(
                Success: false, 
                Artifact: MapToDto(artifact),
                Movement: null, 
                HasViolations: true,
                Violations: violations,
                Message: "El movimiento viola reglas de OpenUP. Se requiere confirmación."
            );
        }

        // Registrar movimiento
        var movement = new ArtifactMovementHistory
        {
            Id = Guid.NewGuid(),
            ArtifactId = artifactId,
            MovementType = "PHASE_CHANGE",
            FromPhaseId = oldPhaseId,
            ToPhaseId = dto.NewPhaseId,
            Reason = dto.Reason,
            MovedBy = userId,
            MovedAt = DateTime.UtcNow,
            ViolatedRules = violations.Any(),
            ViolationDetails = violations.Any() ? JsonSerializer.Serialize(violations) : null
        };

        await _movementHistoryRepository.CreateAsync(movement);

        // Actualizar artefacto
        artifact.PhaseId = dto.NewPhaseId;
        var updatedArtifact = await _artifactRepository.UpdateAsync(artifact);

        return new ReassignmentResultDto(
            Success: true,
            Artifact: MapToDto(updatedArtifact),
            Movement: MapToMovementDto(movement),
            HasViolations: violations.Any(),
            Violations: violations,
            Message: $"Artefacto movido de {oldPhaseId} a {dto.NewPhaseId}" + 
                (violations.Any() ? " (con violaciones confirmadas)" : "")
        );
    }

    public async Task<ReassignmentResultDto> ReassignWorkflowAsync(Guid artifactId, string userId, ReassignWorkflowDto dto)
    {
        var artifact = await _artifactRepository.GetByIdAsync(artifactId);
        if (artifact == null)
        {
            return new ReassignmentResultDto(
                Success: false, 
                Artifact: null,
                Movement: null, 
                HasViolations: false,
                Violations: new List<ReassignmentViolationDto>(),
                Message: "Artefacto no encontrado"
            );
        }

        var oldWorkflowId = artifact.WorkflowId;
        var oldStateId = artifact.CurrentStateId;

        // Registrar movimiento
        var movement = new ArtifactMovementHistory
        {
            Id = Guid.NewGuid(),
            ArtifactId = artifactId,
            MovementType = "WORKFLOW_CHANGE",
            FromWorkflowId = oldWorkflowId,
            ToWorkflowId = dto.NewWorkflowId,
            FromStateId = oldStateId,
            ToStateId = dto.NewStateId,
            Reason = dto.Reason,
            MovedBy = userId,
            MovedAt = DateTime.UtcNow,
            ViolatedRules = false,
            ViolationDetails = null
        };

        await _movementHistoryRepository.CreateAsync(movement);

        // Actualizar artefacto
        artifact.WorkflowId = dto.NewWorkflowId;
        artifact.CurrentStateId = dto.NewStateId;
        var updatedArtifact = await _artifactRepository.UpdateAsync(artifact);

        return new ReassignmentResultDto(
            Success: true,
            Artifact: MapToDto(updatedArtifact),
            Movement: MapToMovementDto(movement),
            HasViolations: false,
            Violations: new List<ReassignmentViolationDto>(),
            Message: "Workflow del artefacto actualizado"
        );
    }

    public async Task<ReassignmentResultDto> ValidateReassignmentAsync(Guid artifactId, ValidateReassignmentDto dto)
    {
        var artifact = await _artifactRepository.GetByIdAsync(artifactId);
        if (artifact == null)
        {
            return new ReassignmentResultDto(
                Success: false, 
                Artifact: null,
                Movement: null, 
                HasViolations: false,
                Violations: new List<ReassignmentViolationDto>(),
                Message: "Artefacto no encontrado"
            );
        }

        var violations = new List<ReassignmentViolationDto>();

        if (!string.IsNullOrEmpty(dto.NewPhaseId))
        {
            violations = ValidatePhaseChange(artifact.PhaseId, dto.NewPhaseId);
        }

        return new ReassignmentResultDto(
            Success: !violations.Any(),
            Artifact: MapToDto(artifact),
            Movement: null,
            HasViolations: violations.Any(),
            Violations: violations,
            Message: violations.Any() 
                ? "El movimiento tiene violaciones de reglas" 
                : "El movimiento es válido"
        );
    }

    public async Task<IEnumerable<ArtifactMovementHistoryDto>> GetMovementHistoryAsync(Guid artifactId)
    {
        var movements = await _movementHistoryRepository.GetByArtifactIdAsync(artifactId);
        return movements.Select(MapToMovementDto);
    }

    private List<ReassignmentViolationDto> ValidatePhaseChange(string fromPhase, string toPhase)
    {
        var violations = new List<ReassignmentViolationDto>();

        var fromIndex = Array.IndexOf(ValidPhaseOrder, fromPhase.ToUpperInvariant());
        var toIndex = Array.IndexOf(ValidPhaseOrder, toPhase.ToUpperInvariant());

        // Fase no válida
        if (fromIndex == -1 || toIndex == -1)
        {
            violations.Add(new ReassignmentViolationDto(
                "INVALID_PHASE",
                "Una de las fases especificadas no es válida para OpenUP",
                "error"
            ));
            return violations;
        }

        // Movimiento hacia atrás (retroceso de fase)
        if (toIndex < fromIndex)
        {
            violations.Add(new ReassignmentViolationDto(
                "BACKWARD_PHASE_MOVE",
                $"Mover un artefacto de {fromPhase} a {toPhase} implica un retroceso en el ciclo de vida OpenUP",
                "warning"
            ));
        }

        // Saltar fases (movimiento no secuencial)
        if (Math.Abs(toIndex - fromIndex) > 1)
        {
            violations.Add(new ReassignmentViolationDto(
                "SKIP_PHASE",
                $"Se están saltando fases intermedias en el movimiento de {fromPhase} a {toPhase}",
                "info"
            ));
        }

        return violations;
    }

    private static ArtifactMovementHistoryDto MapToMovementDto(ArtifactMovementHistory movement)
    {
        return new ArtifactMovementHistoryDto(
            movement.Id,
            movement.ArtifactId,
            movement.MovementType,
            movement.FromPhaseId,
            movement.ToPhaseId,
            movement.FromWorkflowId,
            movement.ToWorkflowId,
            movement.FromStateId,
            movement.ToStateId,
            movement.Reason,
            movement.MovedBy,
            movement.MovedAt,
            movement.ViolatedRules,
            movement.ViolationDetails
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
