using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using OpenUpTool.Infrastructure.Data;

namespace OpenUpTool.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly OpenUpToolDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(OpenUpToolDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        _logger.LogInformation("Intentando login para email: {Email}", dto.Email);
        
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

        if (user == null)
        {
            _logger.LogWarning("Usuario no encontrado: {Email}", dto.Email);
            return null;
        }
        
        if (!user.IsActive)
        {
            _logger.LogWarning("Usuario inactivo: {Email}", dto.Email);
            return null;
        }

        _logger.LogInformation("Usuario encontrado, verificando password. Hash almacenado: {Hash}", user.PasswordHash.Substring(0, 20));
        
        // Verificar password con BCrypt
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        _logger.LogInformation("Resultado de verificación BCrypt: {IsValid}", isPasswordValid);
        
        if (!isPasswordValid)
        {
            _logger.LogWarning("Password incorrecto para usuario: {Email}", dto.Email);
            return null;
        }

        // Actualizar último login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Generar JWT token
        var token = GenerateJwtToken(user);

        _logger.LogInformation("Login exitoso para usuario: {Email}", dto.Email);
        
        return new LoginResponseDto(
            token,
            MapToUserDto(user)
        );
    }

    public async Task<UserDto?> RegisterAsync(RegisterDto dto)
    {
        // Verificar si el email ya existe
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
            return null;

        // Hash del password con BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email.ToLower(),
            PasswordHash = passwordHash,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            RoleId = dto.RoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Recargar con el rol
        await _context.Entry(user).Reference(u => u.Role).LoadAsync();

        return MapToUserDto(user);
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        return user == null ? null : MapToUserDto(user);
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _context.Roles.ToListAsync();
        return roles.Select(r => new RoleDto(r.Id, r.Name, r.Description));
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSecret = _configuration["JWT_SECRET"] ?? 
                       Environment.GetEnvironmentVariable("JWT_SECRET") ?? 
                       "your-super-secret-key-change-this-in-production-min-32-chars";
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role.Name)
        };

        var token = new JwtSecurityToken(
            issuer: "OpenUpTool",
            audience: "OpenUpTool",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.Name,
            user.IsActive,
            user.CreatedAt
        );
    }
}
