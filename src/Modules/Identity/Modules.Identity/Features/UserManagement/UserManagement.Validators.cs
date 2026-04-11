using FluentValidation;
using Modules.Identity.Constants;

namespace Modules.Identity.Features.UserManagement;

internal sealed class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    private static readonly string[] ValidRoles =
    [
        RoleConstants.SuperAdmin,
        RoleConstants.SupportAdmin,
        RoleConstants.QuizAuthor,
        RoleConstants.Examine
    ];

    public UpdateUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.RoleName)
            .NotNull()
            .NotEmpty()
            .WithMessage("Role name is required")
            .Must(role => ValidRoles.Contains(role))
            .WithMessage($"Role must be one of: {string.Join(", ", ValidRoles)}");
    }
}
