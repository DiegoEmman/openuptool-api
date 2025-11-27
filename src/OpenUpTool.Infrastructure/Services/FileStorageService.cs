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
        int versionNumber, 
        Stream fileStream,
        string fileName)
    {
        try
        {
            // Crear estructura: uploads/{projectId}/{artifactId}/v{number}/
            var projectPath = Path.Combine(_baseUploadPath, projectId.ToString());
            var artifactPath = Path.Combine(projectPath, artifactId.ToString());
            var versionPath = Path.Combine(artifactPath, $"v{versionNumber}");

            // Crear directorios si no existen
            Directory.CreateDirectory(versionPath);

            // Sanitizar nombre de archivo
            var safeName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            
            // Generar timestamp para evitar colisiones
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var finalFileName = $"{timestamp}_{safeName}";
            var fullPath = Path.Combine(versionPath, finalFileName);

            // Guardar archivo
            long fileSize;
            using (var fileStreamOutput = new FileStream(fullPath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
                fileSize = fileStreamOutput.Length;
            }

            _logger.LogInformation("Archivo guardado: {FilePath}", fullPath);

            // Retornar ruta relativa desde wwwroot/uploads
            var relativePath = Path.Combine(projectId.ToString(), artifactId.ToString(), $"v{versionNumber}", finalFileName);
            return (relativePath, finalFileName, fileSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar archivo para proyecto {ProjectId}, artefacto {ArtifactId}, versión {Version}", 
                projectId, artifactId, versionNumber);
            throw;
        }
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
