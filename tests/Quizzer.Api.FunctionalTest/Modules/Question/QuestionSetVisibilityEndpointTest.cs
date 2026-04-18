using FluentAssertions;
using Modules.Quiz.Application.Question.Question.Create;
using Modules.Quiz.Application.Question.QuestionSet.Create;
using Modules.Quiz.Application.Question.QuestionSet.Dtos;
using Modules.Quiz.Application.Question.QuestionSet.Update;
using Modules.Quiz.Core;
using Quizzer.Api.FunctionalTest.Abstraction;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Quizzer.Api.FunctionalTest.Modules.Question;

public class QuestionSetVisibilityEndpointTest : QuizzerBaseFunctionTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    public QuestionSetVisibilityEndpointTest(QuizzerWebApiFactory factory, ITestOutputHelper testOutputHelper) : base(factory)
    {
        _testOutputHelper = testOutputHelper;
        RegisterOneTimeUser().Wait();
        LoginOneTimeUser().Wait();
    }

    [Fact]
    public async Task ToggleVisibility_SetPublic_ShouldReturnOk()
    {
        // Arrange
        var questionSet = await CreateTestQuestionSet();
        AddTokenToEachRequest();

        // Act
        var toggleCmd = new ToggleVisibilityCommand(questionSet.QuestionSetId, true);
        var response = await HttpClient.PatchAsJsonAsync(
            $"/api/question/questionSet/{questionSet.QuestionSetId}/visibility", toggleCmd);

        _testOutputHelper.WriteLine("ToggleVisibility_SetPublic_ShouldReturnOk Response: {0}", await response.Content.ReadAsStringAsync());
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<QuestionSetResponse>();
        result.Should().NotBeNull();
        result!.IsPublic.Should().BeTrue();
        result.QuestionSetId.Should().Be(questionSet.QuestionSetId);
    }

    [Fact]
    public async Task ToggleVisibility_SetPrivate_ShouldReturnOk()
    {
        // Arrange
        var questionSet = await CreateTestQuestionSet();
        AddTokenToEachRequest();

        // Toggle to public first
        var togglePublicCmd = new ToggleVisibilityCommand(questionSet.QuestionSetId, true);
        var publicResponse = await HttpClient.PatchAsJsonAsync(
            $"/api/question/questionSet/{questionSet.QuestionSetId}/visibility", togglePublicCmd);
        publicResponse.EnsureSuccessStatusCode();

        // Act — toggle back to private
        var togglePrivateCmd = new ToggleVisibilityCommand(questionSet.QuestionSetId, false);
        var response = await HttpClient.PatchAsJsonAsync(
            $"/api/question/questionSet/{questionSet.QuestionSetId}/visibility", togglePrivateCmd);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<QuestionSetResponse>();
        result.Should().NotBeNull();
        result!.IsPublic.Should().BeFalse();
        result.QuestionSetId.Should().Be(questionSet.QuestionSetId);
    }

    [Fact]
    public async Task ToggleVisibility_NonExistentQuestionSet_ShouldReturnNotFound()
    {
        // Arrange
        AddTokenToEachRequest();
        const long nonExistentId = 99999;
        var toggleCmd = new ToggleVisibilityCommand(nonExistentId, true);

        // Act
        var response = await HttpClient.PatchAsJsonAsync(
            $"/api/question/questionSet/{nonExistentId}/visibility", toggleCmd);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ToggleVisibility_MismatchedIds_ShouldReturnBadRequest()
    {
        // Arrange
        var questionSet = await CreateTestQuestionSet();
        AddTokenToEachRequest();

        var mismatchedRouteId = questionSet.QuestionSetId + 1;
        var toggleCmd = new ToggleVisibilityCommand(questionSet.QuestionSetId, true);

        // Act
        var response = await HttpClient.PatchAsJsonAsync(
            $"/api/question/questionSet/{mismatchedRouteId}/visibility", toggleCmd);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ToggleVisibility_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange — do not call AddTokenToEachRequest()
        HttpClient.DefaultRequestHeaders.Authorization = null;
        const long questionSetId = 1;
        var toggleCmd = new ToggleVisibilityCommand(questionSetId, true);

        // Act
        var response = await HttpClient.PatchAsJsonAsync(
            $"/api/question/questionSet/{questionSetId}/visibility", toggleCmd);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<QuestionSetResponse> CreateTestQuestionSet(string? setCode = null)
    {
        AddTokenToEachRequest();
        setCode ??= $"VIS-{Guid.NewGuid():N}"[..10];
        var questionCommands = new List<CreateQuestionCommand>
        {
            new($"{Guid.NewGuid():N}", "Question details", 5,
            [
                new("Option A", false),
                new("Option B", true),
                new("Option C", false),
                new("Option D", false),
            ]),
        };
        var createCmd = new CreateQuestionSetCommand($"{Guid.NewGuid():N}", setCode, "Test description", questionCommands);
        var response = await HttpClient.PostAsJsonAsync(
            QuestionModuleConstants.Route.QuestionSetRoute.CreateQuestionSet, createCmd);
        _testOutputHelper.WriteLine("CreateTestQuestionSet Response: {0}", await response.Content.ReadAsStringAsync());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<QuestionSetResponse>())!;
    }
}
