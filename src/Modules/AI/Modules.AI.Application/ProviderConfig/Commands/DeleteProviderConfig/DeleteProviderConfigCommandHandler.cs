using Modules.AI.Core.Errors;
using Modules.AI.Core.Repositories;
using Shared.Core;

namespace Modules.AI.Application.ProviderConfig.Commands.DeleteProviderConfig;

internal sealed class DeleteProviderConfigCommandHandler(
    IAIProviderConfigRepository repository,
    IUser user)
    : ICommandHandler<DeleteProviderConfigCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        DeleteProviderConfigCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            return Error.Unauthorized("User.NotAuthenticated", "User is not authenticated.");
        }

        var userId = Guid.Parse(user.Id);
        var existing = await repository.GetByUserIdAsync(userId, cancellationToken);

        if (existing is null)
        {
            return AIProviderErrors.ProviderNotConfigured;
        }

        await repository.DeleteByUserIdAsync(userId, cancellationToken);

        return true;
    }
}
