using Shared.Core;

namespace Modules.Identity.Features.Registration;
internal sealed record UserRegistrationCommand(
    string FullName,
    string Email,
    string Password
    ) : ICommand<Result<bool>>;
