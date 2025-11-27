using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OpenUpTool.Tests.Helpers;

/// <summary>
/// Helper para generar tokens JWT de prueba
/// </summary>
public static class JwtTokenHelper
{
    private const string SecretKey = "ThisIsATestSecretKeyForJWTTokens123456";

    /// <summary>
    /// Genera un token JWT de prueba para un usuario específico
    /// </summary>
    public static string GenerateTestToken(Guid userId, string email, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(SecretKey);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Genera un token para un usuario Admin
    /// </summary>
    public static string GenerateAdminToken()
    {
        return GenerateTestToken(
            TestDataSeeder.AdminUserId,
            "admin@test.com",
            "Admin"
        );
    }

    /// <summary>
    /// Genera un token para un usuario Manager
    /// </summary>
    public static string GenerateManagerToken()
    {
        return GenerateTestToken(
            TestDataSeeder.ManagerUserId,
            "manager@test.com",
            "Manager"
        );
    }

    /// <summary>
    /// Genera un token para un usuario Developer
    /// </summary>
    public static string GenerateDeveloperToken()
    {
        return GenerateTestToken(
            TestDataSeeder.DeveloperUserId,
            "developer@test.com",
            "Developer"
        );
    }
}
