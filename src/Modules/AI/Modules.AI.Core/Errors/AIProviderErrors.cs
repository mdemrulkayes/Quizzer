using Shared.Core;

namespace Modules.AI.Core.Errors;

public struct AIProviderErrors
{
    public static readonly Error ProviderNotConfigured = Error.NotFound(
        "AIProvider.NotConfigured",
        "No AI provider has been configured. Please set up your AI provider in settings.");

    public static readonly Error ProviderNotSupported = Error.Validation(
        "AIProvider.NotSupported",
        "The specified AI provider is not supported.");

    public static readonly Error ConnectionTestFailed = Error.Failure(
        "AIProvider.ConnectionTestFailed",
        "Failed to connect to the AI provider. Please verify your API key.");

    public static readonly Error InvalidApiKey = Error.Validation(
        "AIProvider.InvalidApiKey",
        "The API key cannot be empty.");

    public static Error ApiError(string message) =>
        Error.Failure("AIProvider.ApiError", message);

    public static Error RateLimitExceeded(string message) =>
        Error.Failure("AIProvider.RateLimitExceeded", message);

    public static Error AuthenticationFailed(string message) =>
        Error.Unauthorized("AIProvider.AuthenticationFailed", message);

    public static Error ServerError(string message) =>
        Error.Unexpected("AIProvider.ServerError", message);
}
