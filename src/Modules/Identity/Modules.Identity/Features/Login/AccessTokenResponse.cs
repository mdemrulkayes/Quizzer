namespace Modules.Identity.Features.Login;

internal sealed record AccessTokenResponse(
    string Token,
    string TokenType,
    string RefreshToken,
    DateTime RefreshTokenExpiryDate,
    long ExpiresIn = 3600);