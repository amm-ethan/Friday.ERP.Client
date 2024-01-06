using Friday.ERP.Client.Data.RequestFeatures;

namespace Friday.ERP.Client.Data.DataTransferObjects;

public record UserCreateDto(
    string Name,
    string? Phone,
    string? Email,
    string Username,
    byte[] Password,
    Guid RoleGuid);

public record UserViewDto(
    Guid Guid,
    string Name,
    string? Phone,
    string? Email,
    string Username,
    Guid RoleGuid,
    string RoleName);

public record UserUpdateDto(
    string? Name,
    string? Phone,
    string? Email,
    bool? IsForceReset,
    byte[]? OldPassword,
    byte[]? NewPassword,
    Guid? RoleGuid);

public class UserParameter : RequestParameters
{
    public UserParameter()
    {
        OrderBy = "Username";
    }

    public string? SearchTerm { get; set; }
}

public record UserRoleViewDto(
    Guid Guid,
    string Name
);