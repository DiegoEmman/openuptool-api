using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Core.Services;

public class ArtifactVersionService : IArtifactVersionService
{
    private readonly IArtifactVersionRepository _versionRepository;
    private readonly IArtifactRepository _artifactRepository;
    private readonly IFileStorageService _fileStorageService;

    public ArtifactVersionService(
        IArtifactVersionRepository versionRepository,
        IArtifactRepository artifactRepository,
        IFileStorageService fileStorageService)
    {
        _versionRepository = versionRepository;
        _artifactRepository = artifactRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<ArtifactVersionDto> CreateVersionAsync(
        Guid artifactId, 
        CreateArtifactVersionDto dto, 
        Stream? fileStream = null, 
        string? fileName = null)
    {
        // Verify artifact exists
        var artifact = await _artifactRepository.GetByIdAsync(artifactId);
        if (artifact == null)
        {
            throw new InvalidOperationException("Artefacto no encontrado");
        }

        // Get next version number
        var existingVersions = await _versionRepository.GetVersionsByArtifactIdAsync(artifactId);
        var nextVersionNumber = existingVersions.Any() 
            ? existingVersions.Max(v => v.VersionNumber) + 1 
            : 1;

        // Create version entity
        var version = new ArtifactVersion
        {
            Id = Guid.NewGuid(),
            ArtifactId = artifactId,
            VersionNumber = nextVersionNumber,
            UploadedBy = dto.UploadedBy,
            UploadedAt = DateTime.UtcNow,
            ChangeDescription = dto.ChangeDescription?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Save file if provided
        if (fileStream != null && !string.IsNullOrEmpty(fileName))
        {
            var (filePath, savedFileName, fileSize) = await _fileStorageService.SaveFileAsync(
                artifact.ProjectId,
                artifactId,
                fileStream,
                fileName,
                "versions"
            );

            version.FilePath = filePath;
            version.FileName = savedFileName;
            version.FileSize = fileSize;
        }

        // Save version to database
        await _versionRepository.AddAsync(version);

        return MapToDto(version);
    }

    public async Task<VersionHistoryDto> GetVersionHistoryAsync(Guid artifactId)
    {
        // Verify artifact exists
        var artifact = await _artifactRepository.GetByIdAsync(artifactId);
        if (artifact == null)
        {
            throw new InvalidOperationException("Artefacto no encontrado");
        }

        // Get all versions
        var versions = await _versionRepository.GetVersionsByArtifactIdAsync(artifactId);
        var versionList = versions
            .OrderByDescending(v => v.VersionNumber)
            .Select(MapToDto)
            .ToList();

        return new VersionHistoryDto(
            ArtifactId: artifactId,
            ArtifactTitle: artifact.Title,
            TotalVersions: versionList.Count,
            Versions: versionList
        );
    }

    public async Task<ArtifactVersionDto?> GetVersionByIdAsync(Guid versionId)
    {
        var version = await _versionRepository.GetByIdAsync(versionId);
        return version != null ? MapToDto(version) : null;
    }

    public async Task<VersionComparisonDto?> CompareVersionsAsync(Guid versionId1, Guid versionId2)
    {
        var version1 = await _versionRepository.GetByIdAsync(versionId1);
        var version2 = await _versionRepository.GetByIdAsync(versionId2);

        if (version1 == null || version2 == null)
        {
            return null;
        }

        if (version1.ArtifactId != version2.ArtifactId)
        {
            throw new InvalidOperationException("Las versiones no pertenecen al mismo artefacto");
        }

        var differences = CalculateDifferences(version1, version2);

        return new VersionComparisonDto(
            Version1: MapToDto(version1),
            Version2: MapToDto(version2),
            Differences: differences
        );
    }

    public async Task<Stream?> GetVersionFileAsync(Guid versionId)
    {
        var version = await _versionRepository.GetByIdAsync(versionId);
        if (version == null || string.IsNullOrEmpty(version.FilePath))
        {
            return null;
        }

        return await _fileStorageService.GetFileStreamAsync(version.FilePath);
    }

    private static VersionDifferencesDto CalculateDifferences(ArtifactVersion v1, ArtifactVersion v2)
    {
        var fileChanged = v1.FileName != v2.FileName || v1.FileSize != v2.FileSize;
        var fileSizeChanged = v1.FileSize != v2.FileSize;
        var fileSizeDifference = fileSizeChanged && v1.FileSize.HasValue && v2.FileSize.HasValue
            ? (long?)(v2.FileSize.Value - v1.FileSize.Value)
            : null;
        var authorChanged = v1.UploadedBy != v2.UploadedBy;
        var timeDifference = v2.UploadedAt - v1.UploadedAt;

        return new VersionDifferencesDto(
            FileChanged: fileChanged,
            FileSizeChanged: fileSizeChanged,
            FileSizeDifference: fileSizeDifference,
            AuthorChanged: authorChanged,
            TimeDifference: timeDifference,
            ChangeDescription: v2.ChangeDescription
        );
    }

    private static ArtifactVersionDto MapToDto(ArtifactVersion version)
    {
        return new ArtifactVersionDto(
            Id: version.Id,
            ArtifactId: version.ArtifactId,
            VersionNumber: version.VersionNumber,
            FilePath: version.FilePath,
            FileName: version.FileName,
            FileSize: version.FileSize,
            UploadedBy: version.UploadedBy,
            UploadedAt: version.UploadedAt,
            ChangeDescription: version.ChangeDescription,
            CreatedAt: version.CreatedAt
        );
    }
}
