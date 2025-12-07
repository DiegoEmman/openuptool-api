using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using OpenUpTool.Infrastructure.Data;
using System.Security.Claims;

namespace OpenUpTool.Api.Attributes;

/// <summary>
/// Atributo para validar permisos de workflow antes de ejecutar una acción
/// Bloquea acciones no permitidas y muestra mensaje explicativo
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RequireWorkflowPermissionAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _action;
    private readonly string _workflowIdParameterName;

    /// <summary>
    /// Constructor del atributo
    /// </summary>
    /// <param name="action">Acción a validar (crear, editar, aprobar, cambiar_estado)</param>
    /// <param name="workflowIdParameterName">Nombre del parámetro que contiene el workflowId (default: "workflowId")</param>
    public RequireWorkflowPermissionAttribute(string action, string workflowIdParameterName = "workflowId")
    {
        _action = action;
        _workflowIdParameterName = workflowIdParameterName;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Obtener servicios del DI container
        var dbContext = context.HttpContext.RequestServices.GetService<OpenUpToolDbContext>();
        if (dbContext == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        // Obtener workflowId del route, query o body
        Guid workflowId = Guid.Empty;
        
        // Intentar obtener de route parameters
        if (context.RouteData.Values.ContainsKey(_workflowIdParameterName))
        {
            Guid.TryParse(context.RouteData.Values[_workflowIdParameterName]?.ToString(), out workflowId);
        }
        
        // Intentar obtener de query string
        if (workflowId == Guid.Empty && context.HttpContext.Request.Query.ContainsKey(_workflowIdParameterName))
        {
            Guid.TryParse(context.HttpContext.Request.Query[_workflowIdParameterName].ToString(), out workflowId);
        }
        
        // Intentar obtener del body (si es un DTO)
        if (workflowId == Guid.Empty && context.ActionArguments.Count > 0)
        {
            var firstArg = context.ActionArguments.Values.FirstOrDefault();
            if (firstArg != null)
            {
                var workflowIdProperty = firstArg.GetType().GetProperty("WorkflowId");
                if (workflowIdProperty != null)
                {
                    var value = workflowIdProperty.GetValue(firstArg);
                    if (value is Guid guid)
                    {
                        workflowId = guid;
                    }
                }
                
                // Si no tiene WorkflowId, intentar obtener de ArtifactId
                if (workflowId == Guid.Empty)
                {
                    var artifactIdProperty = firstArg.GetType().GetProperty("ArtifactId");
                    if (artifactIdProperty != null)
                    {
                        var artifactId = artifactIdProperty.GetValue(firstArg);
                        if (artifactId is Guid artGuid && artGuid != Guid.Empty)
                        {
                            // Buscar el workflow del artefacto
                            var artifact = await dbContext.Artifacts
                                .Where(a => a.Id == artGuid)
                                .Select(a => new { a.WorkflowId })
                                .FirstOrDefaultAsync();
                            
                            if (artifact?.WorkflowId != null)
                            {
                                workflowId = artifact.WorkflowId.Value;
                            }
                        }
                    }
                }
            }
        }

        if (workflowId == Guid.Empty)
        {
            context.Result = new BadRequestObjectResult(new
            {
                message = "No se pudo determinar el workflow para validar permisos",
                error = "WorkflowId requerido"
            });
            return;
        }

        // Obtener el rol del usuario desde el token
        var userRole = context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(userRole))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "No se pudo determinar el rol del usuario",
                error = "Rol no encontrado en el token"
            });
            return;
        }

        // Mapear roles del sistema a roles de workflow
        var workflowRole = MapSystemRoleToWorkflowRole(userRole);

        // Verificar permiso en la base de datos
        var permission = await dbContext.WorkflowPermissions
            .Where(p => p.WorkflowId == workflowId 
                     && p.Role == workflowRole 
                     && p.Action == _action)
            .FirstOrDefaultAsync();

        // Si no existe el permiso o está denegado, retornar 403
        if (permission == null || !permission.IsAllowed)
        {
            context.Result = new ObjectResult(new
            {
                message = "No tiene permiso para realizar esta acción",
                error = $"El rol '{workflowRole}' no puede '{_action}' en este workflow",
                details = new
                {
                    rol = workflowRole,
                    accion = _action,
                    workflow = workflowId,
                    modoLectura = true,
                    sugerencia = "Puede ver el contenido en modo solo lectura pero no puede realizar esta acción. Contacte al administrador si necesita permisos adicionales."
                }
            })
            {
                StatusCode = 403
            };
            return;
        }

        // Si tiene permiso, continuar con la ejecución
        await next();
    }

    /// <summary>
    /// Mapea los roles del sistema a roles de workflow
    /// </summary>
    private string MapSystemRoleToWorkflowRole(string systemRole)
    {
        return systemRole.ToLower() switch
        {
            "admin" => "admin",
            "manager" => "PO",
            "developer" => "autor",
            "tester" => "revisor",
            _ => "autor" // Por defecto
        };
    }
}
