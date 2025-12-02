using OpenUpTool.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace OpenUpTool.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _baseUploadPath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(string baseUploadPath, ILogger<FileStorageService> logger)
    {
        _baseUploadPath = baseUploadPath;
        _logger = logger;
        
        // Asegurar que el directorio base existe
        if (!Directory.Exists(_baseUploadPath))
        {
            Directory.CreateDirectory(_baseUploadPath);
        }
    }

    public async Task<(string FilePath, string FileName, long FileSize)> SaveFileAsync(
        Guid projectId, 
        Guid artifactId, 
        Stream fileStream,
        string fileName,
        string category)
    {
        try
        {
            // Crear estructura: uploads/{projectId}/{artifactId}/
            var projectPath = Path.Combine(_baseUploadPath, projectId.ToString());
            var artifactPath = Path.Combine(projectPath, artifactId.ToString());

            // Crear directorios si no existen
            Directory.CreateDirectory(artifactPath);

            // Sanitizar nombre de archivo
            var safeName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            
            // Generar timestamp para evitar colisiones
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var extension = Path.GetExtension(safeName);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(safeName);
            var finalFileName = $"{nameWithoutExt}_{timestamp}{extension}";
            var fullPath = Path.Combine(artifactPath, finalFileName);

            // Guardar archivo
            long fileSize;
            using (var fileStreamOutput = new FileStream(fullPath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
                fileSize = fileStreamOutput.Length;
            }

            _logger.LogInformation("Archivo guardado: {FilePath}, Tamaño: {FileSize} bytes", fullPath, fileSize);

            // Retornar ruta relativa desde uploads
            var relativePath = Path.Combine(projectId.ToString(), artifactId.ToString(), finalFileName).Replace("\\", "/");
            return (relativePath, finalFileName, fileSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar archivo para proyecto {ProjectId}, artefacto {ArtifactId}", 
                projectId, artifactId);
            throw;
        }
    }

    public bool IsValidFileFormat(string fileName, string category)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return category.ToUpper() switch
        {
            "DIAGRAM" => new[] { ".png", ".svg", ".pdf", ".jpg", ".jpeg" }.Contains(extension),
            "PROTOTYPE" => new[] { ".png", ".jpg", ".jpeg", ".gif", ".pdf" }.Contains(extension),
            "DOCUMENT" => new[] { ".pdf", ".docx", ".doc", ".txt" }.Contains(extension),
            _ => false
        };
    }

    public long GetMaxFileSizeForCategory(string category)
    {
        return category.ToUpper() switch
        {
            "DIAGRAM" => 10 * 1024 * 1024, // 10 MB
            "PROTOTYPE" => 15 * 1024 * 1024, // 15 MB
            "DOCUMENT" => 20 * 1024 * 1024, // 20 MB
            _ => 5 * 1024 * 1024 // 5 MB por defecto
        };
    }

    public async Task<Stream?> GetFileStreamAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_baseUploadPath, filePath);
            
            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("Archivo no encontrado: {FilePath}", fullPath);
                return null;
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            
            return memory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener archivo: {FilePath}", filePath);
            throw;
        }
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_baseUploadPath, filePath);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Archivo eliminado: {FilePath}", fullPath);
                return Task.FromResult(true);
            }

            _logger.LogWarning("Archivo no encontrado para eliminar: {FilePath}", fullPath);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar archivo: {FilePath}", filePath);
            throw;
        }
    }

    public Task<string> GetFilePhysicalPathAsync(string filePath)
    {
        var fullPath = Path.Combine(_baseUploadPath, filePath);
        return Task.FromResult(fullPath);
    }
}
