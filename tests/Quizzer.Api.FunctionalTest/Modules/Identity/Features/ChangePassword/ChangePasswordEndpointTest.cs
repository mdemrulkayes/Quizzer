using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Modules.Identity.Constants;
using Modules.Identity.Features.ChangePassword;
using Modules.Identity.Features.Login;
using Quizzer.Api.FunctionalTest.Abstraction;
using Xunit.Abstractions;

namespace Quizzer.Api.FunctionalTest.Modules.Identity.Features.ChangePassword;

public class ChangePasswordEndpointTest : QuizzerBaseFunctionTest
{
    private readonly ITestOutputHelper _output;
    public ChangePasswordEndpointTest(QuizzerWebApiFactory factory, ITestOutputHelper output) : base(factory)
    {
        _output = output;
        RegisterOneTimeUser().Wait();
    }

    [Fact]
    public async Task Should_ReturnOk_WhenPasswordChangedSuccessfully()
    {
        // Arrange: Login first
        var loginResponse = await HttpClient.PostAsJsonAsync(
            IdentityModuleConstants.Route.Login,
            new LoginCommand("test1@gmail.com", "Aa123456#"));

        _output.WriteLine("Login response: {0}", await loginResponse.Content.ReadAsStringAsync());
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginContent!.Token);

        // Act
        var changePasswordResponse = await HttpClient.PutAsJsonAsync(
            IdentityModuleConstants.Route.ChangePassword,
            new ChangePasswordCommand("Aa123456#", "NewPass123#", "NewPass123#"));
        _output.WriteLine("Change password response: {0}", await changePasswordResponse.Content.ReadAsStringAsync());

        // Assert
        changePasswordResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify: Can login with new password
        var newLoginResponse = await HttpClient.PostAsJsonAsync(
            IdentityModuleConstants.Route.Login,
            new LoginCommand("test1@gmail.com", "NewPass123#"));
        newLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_WhenNotAuthenticated()
    {
        // Act: Call without token
        HttpClient.DefaultRequestHeaders.Authorization = null;
        var response = await HttpClient.PutAsJsonAsync(
            IdentityModuleConstants.Route.ChangePassword,
            new ChangePasswordCommand("old", "new", "new"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
