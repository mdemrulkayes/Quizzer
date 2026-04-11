using Shared.Core;

namespace Modules.Identity.Features.RefreshToken;
internal struct RefreshTokenErrors
{
    internal static Error InvalidToken => Error.Unauthorized("RefreshToken.InvalidToken", "Invalid access token or refresh token");
    internal static Error TokenExpired => Error.Unauthorized("RefreshToken.TokenExpired", "Refresh token has expired");
    internal static Error UserNotFound => Error.NotFound("RefreshToken.UserNotFound", "User not found");
}
