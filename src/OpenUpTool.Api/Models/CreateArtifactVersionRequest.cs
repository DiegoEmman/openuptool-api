namespace OpenUpTool.Api.Models;

/// <summary>
/// Request para crear una nueva versión de un artefacto
/// </summary>
public class CreateArtifactVersionRequest
{
    /// <summary>
    /// Archivo a subir (opcional)
    /// </summary>
    public IFormFile? File { get; set; }

    /// <summary>
    /// Descripción de los cambios en esta versión
    /// </summary>
    public string? ChangeDescription { get; set; }

    /// <summary>
    /// Usuario que sube la versión
    /// </summary>
    public string? UploadedBy { get; set; }
}
