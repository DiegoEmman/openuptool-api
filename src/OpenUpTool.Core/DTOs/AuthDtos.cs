namespace OpenUpTool.Core.DTOs;

public record LoginDto(
    string Email,
    string Password
);

public record RegisterDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    Guid RoleId
);

public record LoginResponseDto(
    string Token,
    UserDto User
);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);

public record RoleDto(
    Guid Id,
    string Name,
    string Description
);
