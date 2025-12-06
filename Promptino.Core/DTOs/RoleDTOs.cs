namespace Promptino.Core.DTOs;

public record CreateRoleDto(string RoleName, string Details);

public record AddUserToRoleDto(Guid UserId, string RoleName);