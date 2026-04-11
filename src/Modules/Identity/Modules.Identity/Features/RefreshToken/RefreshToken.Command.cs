using Shared.Core;

namespace Modules.Identity.Features.RefreshToken;
internal sealed record RefreshTokenCommand(string AccessToken, string RefreshToken) : ICommand<Result<Login.AccessTokenResponse>>;
