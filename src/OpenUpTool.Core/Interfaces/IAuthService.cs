using OpenUpTool.Core.DTOs;

namespace OpenUpTool.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
    Task<UserDto?> RegisterAsync(RegisterDto dto);
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
}
