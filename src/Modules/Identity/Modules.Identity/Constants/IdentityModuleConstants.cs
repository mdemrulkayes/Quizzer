namespace Modules.Identity.Constants;
internal struct IdentityModuleConstants
{
    internal struct Route
    {
        internal const string Register = "/api/identity/register";
        internal const string Login = "/api/identity/login";
        internal static string Profile = "/api/identity/profile";
        internal const string RefreshToken = "/api/identity/refresh-token";
        internal const string ChangePassword = "/api/identity/change-password";
        internal const string GetAllUsers = "/api/identity/users";
        internal const string GetUserById = "/api/identity/users/{userId}";
        internal const string UpdateUserRole = "/api/identity/users/{userId}/role";
        internal const string DeleteUser = "/api/identity/users/{userId}";
    }

    internal struct RouteTag
    {
        internal const string IdentityTagName = "Identity";
        internal const string UserManagementTagName = "User Management";
    }

    internal struct Role
    {
        internal const string SuperAdminRoleGuid = "ac3c30c4-fcbd-4e5a-ab8b-8f6179a65120";
        internal const string SupportAdminRoleGuid = "3123befc-4fd0-4493-b28e-46c1ed881ca4";
        internal const string QuizAuthorRoleGuid = "f0cc1d90-471c-4563-b20a-12acdb47735b";
        internal const string ExamineRoleGuid = "9d476df0-1663-43af-b06b-af945b07db45";
    }

    internal const string MigrationHistoryTableName = "__IdentityModuleMigrationHistory";
    internal const string SchemaName = "Identity";

    internal const string RefreshTokenName = "RefreshToken";
}
