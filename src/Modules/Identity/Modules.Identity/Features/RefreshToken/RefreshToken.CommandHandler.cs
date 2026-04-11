using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Modules.Identity.Constants;
using Modules.Identity.Entities;
using Modules.Identity.Features.Login;
using Shared.Core;

namespace Modules.Identity.Features.RefreshToken;
internal sealed class RefreshTokenCommandHandler(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtConfiguration> jwtConfigurationOptions,
    ILogger<RefreshTokenCommandHandler> logger,
    ITimeProvider timeProvider) : ICommandHandler<RefreshTokenCommand, Result<AccessTokenResponse>>
{
    public async Task<Result<AccessTokenResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var principal = GetPrincipalFromExpiredToken(command.AccessToken);
        if (principal == null)
        {
            return RefreshTokenErrors.InvalidToken;
        }

        var email = principal.FindFirstValue(JwtRegisteredClaimNames.Email)
                    ?? principal.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(email))
        {
            return RefreshTokenErrors.InvalidToken;
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return RefreshTokenErrors.UserNotFound;
        }

        var isValidRefreshToken = await userManager.VerifyUserTokenAsync(
            user, TokenOptions.DefaultProvider, IdentityModuleConstants.RefreshTokenName, command.RefreshToken);

        if (!isValidRefreshToken)
        {
            return RefreshTokenErrors.InvalidToken;
        }

        // Generate new JWT token
        var jwtToken = GenerateJwtToken(user);

        // Rotate refresh token
        await userManager.RemoveAuthenticationTokenAsync(user, TokenOptions.DefaultProvider, IdentityModuleConstants.RefreshTokenName);
        var newRefreshToken = await userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, IdentityModuleConstants.RefreshTokenName);
        await userManager.SetAuthenticationTokenAsync(user, TokenOptions.DefaultProvider, IdentityModuleConstants.RefreshTokenName, newRefreshToken);

        logger.LogInformation("Token refreshed for user {Email}", email);

        return new AccessTokenResponse(
            Token: jwtToken,
            TokenType: "Bearer",
            RefreshToken: newRefreshToken,
            RefreshTokenExpiryDate: timeProvider.TimeNow.AddMinutes(30).DateTime);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            var jwtConfiguration = jwtConfigurationOptions.Value;
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,
                ValidAudience = jwtConfiguration.JwtAudience,
                ValidIssuer = jwtConfiguration.JwtIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.JwtKey))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating expired token");
            return null;
        }
    }

    private string GenerateJwtToken(ApplicationUser userDetails)
    {
        var jwtConfiguration = jwtConfigurationOptions.Value;
        var claims = new List<Claim>();

        if (userDetails.Email is not null)
        {
            claims.Add(new(JwtRegisteredClaimNames.Email, userDetails.Email));
            claims.Add(new(JwtRegisteredClaimNames.Sub, userDetails.Email));
        }

        var userRoles = userManager.GetRolesAsync(userDetails).Result.ToList();
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.Add(new Claim("UserId", userDetails.Id.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.JwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
        var expires = DateTime.UtcNow.AddSeconds(Convert.ToInt32(jwtConfiguration.JwtExpireSeconds));

        var token = new JwtSecurityToken(
            issuer: jwtConfiguration.JwtIssuer,
            audience: jwtConfiguration.JwtAudience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
