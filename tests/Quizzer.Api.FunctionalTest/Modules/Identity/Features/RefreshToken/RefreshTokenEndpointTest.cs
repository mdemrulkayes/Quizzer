using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Modules.Identity.Constants;
using Modules.Identity.Features.Login;
using Modules.Identity.Features.RefreshToken;
using Quizzer.Api.FunctionalTest.Abstraction;

namespace Quizzer.Api.FunctionalTest.Modules.Identity.Features.RefreshToken;

public class RefreshTokenEndpointTest : QuizzerBaseFunctionTest
{
    public RefreshTokenEndpointTest(QuizzerWebApiFactory factory) : base(factory)
    {
        RegisterOneTimeUser().Wait();
    }

    [Fact]
    public async Task Should_ReturnNewTokenPair_WhenValidRefreshTokenProvided()
    {
        // Arrange: Login to get tokens
        var loginResponse = await HttpClient.PostAsJsonAsync(
            IdentityModuleConstants.Route.Login,
            new LoginCommand("test1@gmail.com", "Aa123456#"));
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        loginContent.Should().NotBeNull();

        // Act: Use refresh token to get new tokens
        var refreshResponse = await HttpClient.PostAsJsonAsync(
            IdentityModuleConstants.Route.RefreshToken,
            new RefreshTokenCommand(loginContent!.Token, loginContent.RefreshToken));

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var newTokens = await refreshResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        newTokens.Should().NotBeNull();
        newTokens!.Token.Should().NotBeNullOrWhiteSpace();
        newTokens.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenInvalidRefreshTokenProvided()
    {
        // Arrange: Login to get access token
        var loginResponse = await HttpClient.PostAsJsonAsync(
            IdentityModuleConstants.Route.Login,
            new LoginCommand("test1@gmail.com", "Aa123456#"));
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();

        // Act: Use invalid refresh token
        var refreshResponse = await HttpClient.PostAsJsonAsync(
            IdentityModuleConstants.Route.RefreshToken,
            new RefreshTokenCommand(loginContent!.Token, "invalid-refresh-token"));

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
