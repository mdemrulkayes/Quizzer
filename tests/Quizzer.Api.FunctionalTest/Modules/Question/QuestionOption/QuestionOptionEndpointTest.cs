using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Modules.Identity.Constants;
using Modules.Identity.Features.Login;
using Modules.Quiz.Application.Question.Question.Dtos;
using Modules.Quiz.Core;
using Modules.Quiz.Endpoints.QuestionOption;
using Quizzer.Api.FunctionalTest.Abstraction;

namespace Quizzer.Api.FunctionalTest.Modules.Question.QuestionOption;

public class QuestionOptionEndpointTest : QuizzerBaseFunctionTest
{
    public QuestionOptionEndpointTest(QuizzerWebApiFactory factory) : base(factory)
    {
        RegisterOneTimeUser().Wait();
        LoginAndSetToken().Wait();
    }

    private async Task LoginAndSetToken()
    {
        // Login as QuizAuthor (test2)
        var loginResponse = await HttpClient.PostAsJsonAsync(
            IdentityModuleConstants.Route.Login,
            new LoginCommand("test2@gmail.com", "Aa123456!"));
        var content = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content!.Token);
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_WhenExamineTriesToAddOption()
    {
        // Arrange: Login as Examine user (test1)
        var loginResponse = await HttpClient.PostAsJsonAsync(
            IdentityModuleConstants.Route.Login,
            new LoginCommand("test1@gmail.com", "Aa123456#"));
        var content = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content!.Token);

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            "/api/question/1/options",
            new AddOptionRequest("Test Option", false));

        // Assert: Should be Forbidden since Examine doesn't have QuizAuthorPolicy
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
