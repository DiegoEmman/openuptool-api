using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/permissions")]
public class PermissionsController : ControllerBase
{
    [HttpGet("matrix")]
    public ActionResult<PermissionMatrixDto> GetPermissionMatrix()
    {
        var matrix = new PermissionMatrixDto
        {
            Permissions = new List<PermissionDefinitionDto>
            {
                new() { Action = "crear_proyecto", Description = "Crear nuevos proyectos", Admin = true, Manager = true, Developer = false, Viewer = false },
                new() { Action = "editar_proyecto", Description = "Editar configuración del proyecto", Admin = true, Manager = true, Developer = false, Viewer = false },
                new() { Action = "eliminar_proyecto", Description = "Eliminar/archivar proyectos", Admin = true, Manager = false, Developer = false, Viewer = false },
                new() { Action = "crear_artefacto", Description = "Crear artefactos", Admin = true, Manager = true, Developer = true, Viewer = false },
                new() { Action = "editar_artefacto", Description = "Editar artefactos", Admin = true, Manager = true, Developer = true, Viewer = false }
            }
        };

        return Ok(matrix);
    }
}
