using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Modules.Identity.Constants;
using Modules.Identity.Features.Login;
using Modules.Quiz.Application.Tag.Dtos;
using Modules.Quiz.Core;
using Modules.Quiz.Endpoints.QuestionSetTag;
using Quizzer.Api.FunctionalTest.Abstraction;

namespace Quizzer.Api.FunctionalTest.Modules.Question.QuestionSetTag;

public class QuestionSetTagEndpointTest : QuizzerBaseFunctionTest
{
    public QuestionSetTagEndpointTest(QuizzerWebApiFactory factory) : base(factory)
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
    public async Task Should_AssignAndGetTags_ForQuestionSet()
    {
        // Arrange: Create a tag
        var createTagResponse = await HttpClient.PostAsJsonAsync(
            QuestionModuleConstants.Route.TagRoute.CreateTag,
            new { Name = "TestTag", Description = "A test tag for assignment." });
        createTagResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tag = await createTagResponse.Content.ReadFromJsonAsync<TagResponse>();

        // Create a question set
        var createSetResponse = await HttpClient.PostAsJsonAsync(
            QuestionModuleConstants.Route.QuestionSetRoute.CreateQuestionSet,
            new { Name = "TestSet", SetCode = "TS01", Details = "Test question set" });
        createSetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Get the set ID from response
        var setResponse = await createSetResponse.Content.ReadAsStringAsync();

        // Act: Assign tag to question set (using setId=1 since it's the first one)
        var assignResponse = await HttpClient.PostAsJsonAsync(
            "/api/question/questionSet/1/tags",
            new AssignTagRequest(tag!.TagId));

        // Assert
        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act: Get tags for question set
        var getTagsResponse = await HttpClient.GetAsync("/api/question/questionSet/1/tags");

        // Assert
        getTagsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tags = await getTagsResponse.Content.ReadFromJsonAsync<List<TagResponse>>();
        tags.Should().NotBeNull();
        tags.Should().ContainSingle(t => t.TagId == tag.TagId);
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenAssigningTagToNonExistentQuestionSet()
    {
        // Arrange: Create a tag first
        var createTagResponse = await HttpClient.PostAsJsonAsync(
            QuestionModuleConstants.Route.TagRoute.CreateTag,
            new { Name = "OrphanTag", Description = "Tag with no question set." });
        var tag = await createTagResponse.Content.ReadFromJsonAsync<TagResponse>();

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            "/api/question/questionSet/99999/tags",
            new AssignTagRequest(tag!.TagId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
