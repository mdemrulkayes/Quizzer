using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Modules.Identity.Constants;
using Modules.Identity.Features.Login;
using Quizzer.Api.FunctionalTest.Abstraction;

namespace Quizzer.Api.FunctionalTest.Modules.Identity.Features.UserManagement;

public class UserManagementEndpointTest : QuizzerBaseFunctionTest
{
    public UserManagementEndpointTest(QuizzerWebApiFactory factory) : base(factory)
    {
        RegisterOneTimeUser().Wait();
    }

    [Fact]
    public async Task Should_ReturnForbidden_WhenNonAdminTriesToListUsers()
    {
        // Arrange: Login as regular user (Examine role)
        var loginResponse = await HttpClient.PostAsJsonAsync(
            IdentityModuleConstants.Route.Login,
            new LoginCommand("test1@gmail.com", "Aa123456#"));
        var content = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content!.Token);

        // Act
        var response = await HttpClient.GetAsync($"{IdentityModuleConstants.Route.GetAllUsers}?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
