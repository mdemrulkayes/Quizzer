using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Modules.Identity.Constants;
using Modules.Identity.Features.Login;
using Modules.Identity.Features.Registration;
using Quizzer.Api.FunctionalTest.Abstraction;

namespace Quizzer.Api.FunctionalTest.Modules.Identity.Features.UserManagement;

public class UserManagementEndpointTest : QuizzerBaseFunctionTest
{
    public UserManagementEndpointTest(QuizzerWebApiFactory factory) : base(factory)
    {
        RegisterOneTimeUser().Wait();
        SetupAdminUser().Wait();
    }

    private async Task SetupAdminUser()
    {
        // Register an admin user
        var faker = new Bogus.Faker();
        await HttpClient.PostAsJsonAsync(IdentityModuleConstants.Route.Register,
            new UserRegistrationCommand(faker.Name.FullName(), "admin@test.com", "AdminPass1#"));

        // Promote to SuperAdmin via UserManager
        var user = await UserManager.FindByEmailAsync("admin@test.com");
        if (user != null)
        {
            await UserManager.AddToRoleAsync(user, RoleConstants.SuperAdmin);
        }
    }

    private async Task<string> GetAdminToken()
    {
        var loginResponse = await HttpClient.PostAsJsonAsync(
            IdentityModuleConstants.Route.Login,
            new LoginCommand("admin@test.com", "AdminPass1#"));
        var content = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        return content!.Token;
    }

    [Fact]
    public async Task Should_ReturnUsers_WhenAdminGetsAllUsers()
    {
        // Arrange
        var token = await GetAdminToken();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.GetAsync($"{IdentityModuleConstants.Route.GetAllUsers}?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
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
