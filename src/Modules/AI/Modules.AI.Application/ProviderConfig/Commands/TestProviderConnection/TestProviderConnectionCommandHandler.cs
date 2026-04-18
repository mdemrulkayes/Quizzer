using Modules.AI.Application.Dtos;
using Modules.AI.Core.Errors;
using Modules.AI.Core.Providers;
using Modules.AI.Core.Repositories;
using Shared.Core;

namespace Modules.AI.Application.ProviderConfig.Commands.TestProviderConnection;

internal sealed class TestProviderConnectionCommandHandler(
    IAIProviderFactory providerFactory,
    IAIProviderConfigRepository repository,
    IUser user)
    : ICommandHandler<TestProviderConnectionCommand, Result<TestConnectionResponse>>
{
    public async Task<Result<TestConnectionResponse>> Handle(
        TestProviderConnectionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            return Error.Unauthorized("User.NotAuthenticated", "User is not authenticated.");
        }

        var resolveResult = await providerFactory.ResolveForCurrentUserAsync(cancellationToken);

        if (!resolveResult.IsSuccess)
        {
            return resolveResult.Error;
        }

        var (provider, decryptedKey) = resolveResult.Value;

        var testResult = await provider.TestConnectionAsync(decryptedKey, cancellationToken);

        var userId = Guid.Parse(user.Id);
        var config = await repository.GetByUserIdAsync(userId, cancellationToken);

        if (config is not null)
        {
            config.LastTestedAt = DateTimeOffset.UtcNow;
            config.LastTestResult = testResult.IsSuccess ? "Success" : "Failed";
            await repository.SaveAsync(config, cancellationToken);
        }

        if (!testResult.IsSuccess)
        {
            return new TestConnectionResponse(false, AIProviderErrors.ConnectionTestFailed.Message);
        }

        return new TestConnectionResponse(true, "Connection successful.");
    }
}
