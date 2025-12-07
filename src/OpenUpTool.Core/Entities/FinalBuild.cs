namespace OpenUpTool.Core.Entities;

/// <summary>
/// Registro de build final con identificador único y artefactos binarios o enlaces de descarga
/// </summary>
public class FinalBuild
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    
    // Identificación del build
    public string BuildNumber { get; set; } = string.Empty; // Ejemplo: "v1.0.0-build.123", "RELEASE-2024-001"
    public string Version { get; set; } = string.Empty; // Versión semántica: "1.0.0"
    public string? BuildTag { get; set; } // Tag de Git o similar
    public string? CommitHash { get; set; } // Hash del commit
    
    // Información del build
    public DateTime BuildDate { get; set; }
    public string BuiltBy { get; set; } = string.Empty; // Usuario o sistema CI/CD
    public string? BuildEnvironment { get; set; } // CI/CD: Jenkins, GitHub Actions, Azure DevOps, etc.
    public string? BuildConfiguration { get; set; } // Release, Debug, Production
    
    // Artefactos binarios (JSON array)
    // Estructura: [ { "name": "string", "type": "EXECUTABLE|LIBRARY|PACKAGE|INSTALLER", "filePath": "string", "downloadUrl": "string", "size": number, "checksum": "string", "checksumType": "MD5|SHA256" } ]
    public string BinaryArtifacts { get; set; } = "[]";
    
    // Enlaces de descarga
    public string? MainDownloadUrl { get; set; } // URL principal de descarga (release package)
    public string? DocumentationUrl { get; set; } // Documentación técnica
    public string? ReleaseNotesUrl { get; set; } // Notas de la versión
    
    // Información técnica
    public string? TargetPlatform { get; set; } // Windows, Linux, MacOS, Web, Android, iOS
    public string? Dependencies { get; set; } // JSON con dependencias y versiones
    public string? SystemRequirements { get; set; } // Requisitos del sistema
    
    // Metadata de calidad
    public bool IsStable { get; set; } // true = Stable Release, false = Beta/RC
    public int? TestsPassed { get; set; }
    public int? TestsTotal { get; set; }
    public double? CodeCoverage { get; set; } // Porcentaje 0-100
    public string? QualityGateStatus { get; set; } // Passed, Failed, Warning
    
    // Relación con cierre
    public Guid? ClosureId { get; set; } // Vinculación opcional con ProjectClosure
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public Project Project { get; set; } = null!;
    public ProjectClosure? Closure { get; set; }
}
