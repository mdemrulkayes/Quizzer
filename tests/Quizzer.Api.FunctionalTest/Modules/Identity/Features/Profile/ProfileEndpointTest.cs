using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Modules.Identity.Constants;
using Modules.Identity.Features.Profile;
using Quizzer.Api.FunctionalTest.Abstraction;
using Quizzer.Api.FunctionalTest.DataCollection;

namespace Quizzer.Api.FunctionalTest.Modules.Identity.Features.Profile;

public sealed class ProfileEndpointTest : QuizzerBaseFunctionTest
{
    public ProfileEndpointTest(QuizzerWebApiFactory factory) : base(factory)
    {
        RegisterOneTimeUser().Wait();
        LoginOneTimeUser().Wait();
    }
    [Theory]
    [ClassData(typeof(UserEmailDataCollection))]
    public async Task Should_ReturnLoggedInUserDetails_WhenUserHasValidToken(string userName)
    {
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            LoggedInUserDictionary.FirstOrDefault(x => x.Key == userName).Value);

        var userDataResponse = await HttpClient.GetAsync(IdentityModuleConstants.Route.Profile);
        userDataResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var userProfileData = await userDataResponse.Content.ReadFromJsonAsync<UserProfileResponse>();
        userProfileData.Should().NotBeNull();
        userProfileData.Email.Should().Be(userName);
        userProfileData.FirstName.Should().NotBeNull();
        userProfileData.LastName.Should().NotBeNull();
        userProfileData.Roles.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Should_ReturnUnAuthorized_WhenTokenWillNotBeSupplied()
    {
        var userDataResponse = await HttpClient.GetAsync(IdentityModuleConstants.Route.Profile);
        userDataResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
