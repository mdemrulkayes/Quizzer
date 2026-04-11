using Shared.Core;

namespace Modules.Identity.Features.ChangePassword;
internal sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword) : ICommand<Result<bool>>;
