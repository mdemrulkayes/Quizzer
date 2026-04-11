namespace Modules.Identity.Features.UserManagement;

internal sealed record UserListResponse(
    int TotalCount,
    IReadOnlyCollection<UserListItemResponse> Items);

internal sealed record UserListItemResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    IList<string> Roles,
    bool IsDeleted,
    DateTimeOffset CreatedDate,
    DateTimeOffset? LastLoginTime);

internal sealed record UserDetailResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    IList<string> Roles,
    bool IsDeleted,
    DateTimeOffset CreatedDate,
    DateTimeOffset? UpdatedDate,
    DateTimeOffset? LastLoginTime);
