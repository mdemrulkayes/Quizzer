namespace Modules.Identity.Constants;
public static class RoleConstants
{
    public const string SuperAdmin = "SuperAdmin";
    public const string SupportAdmin = "SupportAdmin";
    public const string QuizAuthor = "QuizAuthor";
    public const string Examine = "Examine";
}

public static class AuthorizationPolicyConstants
{
    public const string SuperAdminPolicy = "SuperAdminPolicy";
    public const string AdminPolicy = "AdminPolicy";
    public const string QuizAuthorPolicy = "QuizAuthorPolicy";
    public const string ExaminePolicy = "ExaminePolicy";
    public const string AuthenticatedPolicy = "AuthenticatedPolicy";
}
