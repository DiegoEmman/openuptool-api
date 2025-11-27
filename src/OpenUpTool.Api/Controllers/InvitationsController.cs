using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;
using System.Security.Claims;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/invitations")]
[Authorize]
public class InvitationsController : ControllerBase
{
    private readonly IProjectInvitationService _service;
    private readonly ILogger<InvitationsController> _logger;

    public InvitationsController(IProjectInvitationService service, ILogger<InvitationsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("my-invitations")]
    public async Task<ActionResult<IEnumerable<ProjectInvitationDto>>> GetMyPendingInvitations()
    {
        try
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var invitations = await _service.GetPendingByEmailAsync(email);
            return Ok(invitations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener invitaciones pendientes");
            return StatusCode(500, new { message = "Error al obtener invitaciones" });
        }
    }

    [HttpGet("project/{projectId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<ProjectInvitationDto>>> GetByProject(Guid projectId)
    {
        try
        {
            var invitations = await _service.GetByProjectIdAsync(projectId);
            return Ok(invitations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener invitaciones del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al obtener invitaciones" });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProjectInvitationDto>> CreateInvitation(CreateInvitationDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var created = await _service.CreateInvitationAsync(dto, userId);
            return CreatedAtAction(nameof(GetByProject), new { projectId = dto.ProjectId }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear invitación");
            return StatusCode(500, new { message = "Error al crear invitación" });
        }
    }

    [HttpPost("accept/{token}")]
    public async Task<IActionResult> AcceptInvitation(string token)
    {
        try
        {
            _logger.LogInformation("Intentando aceptar invitación con token: {Token}", token);
            
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Usuario no autenticado o userId inválido");
                return Unauthorized();
            }

            _logger.LogInformation("Usuario {UserId} aceptando invitación", userId);

            var accepted = await _service.AcceptInvitationAsync(token, userId);
            if (!accepted)
            {
                _logger.LogWarning("Invitación rechazada: token={Token}, userId={UserId}", token, userId);
                return BadRequest(new { message = "Invitación inválida o expirada" });
            }

            _logger.LogInformation("Invitación aceptada exitosamente: token={Token}, userId={UserId}", token, userId);
            return Ok(new { message = "Invitación aceptada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aceptar invitación {Token}", token);
            return StatusCode(500, new { message = "Error al aceptar invitación" });
        }
    }

    [HttpPost("reject/{token}")]
    public async Task<IActionResult> RejectInvitation(string token)
    {
        try
        {
            var rejected = await _service.RejectInvitationAsync(token);
            if (!rejected)
                return BadRequest(new { message = "Invitación inválida" });

            return Ok(new { message = "Invitación rechazada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al rechazar invitación {Token}", token);
            return StatusCode(500, new { message = "Error al rechazar invitación" });
        }
    }

    [HttpDelete("{invitationId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CancelInvitation(Guid invitationId)
    {
        try
        {
            var cancelled = await _service.CancelInvitationAsync(invitationId);
            if (!cancelled)
                return NotFound(new { message = "Invitación no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar invitación {InvitationId}", invitationId);
            return StatusCode(500, new { message = "Error al cancelar invitación" });
        }
    }
}
