using FluentAssertions;
using Modules.AI.Application.Dtos;
using Modules.AI.Application.Generation.Commands.GenerateFromJobDescription;
using Modules.AI.Application.Generation.Commands.GenerateQuestionSet;
using Modules.AI.Application.ProviderConfig.Commands.SaveProviderConfig;
using Modules.AI.Core;
using Quizzer.Api.FunctionalTest.Abstraction;
using System.Net;
using System.Net.Http.Json;

namespace Quizzer.Api.FunctionalTest.Modules.AI;

public class AIGenerationEndpointTest : QuizzerBaseFunctionTest
{
    public AIGenerationEndpointTest(QuizzerWebApiFactory factory) : base(factory)
    {
        RegisterOneTimeUser().Wait();
        LoginOneTimeUser().Wait();
    }

    private async Task EnsureProviderConfigSaved()
    {
        AddTokenToEachRequest();
        var configCmd = new SaveProviderConfigCommand("gemini", "test-api-key-that-is-long-enough");
        await HttpClient.PostAsJsonAsync(AIModuleConstants.Route.ProviderConfig.SaveProviderConfig, configCmd);
    }

    private static string ValidJobDescription =>
        "We are looking for a senior software engineer with experience in .NET, C#, and cloud technologies to join our growing team.";

    #region GenerateQuestionSet Tests

    [Fact]
    public async Task GenerateQuestionSet_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await EnsureProviderConfigSaved();
        var command = new GenerateQuestionSetCommand(
            Topics: ["C#", "ASP.NET Core"],
            Complexity: "intermediate",
            QuestionCount: 10,
            ExperienceYears: null,
            ExpertiseFields: null);

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.Generation.GenerateQuestionSet, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GenerateQuestionSetResponse>();
        result.Should().NotBeNull();
        result!.GenerationRequestId.Should().NotBeEmpty();
        result.Title.Should().NotBeNullOrEmpty();
        result.QuestionCount.Should().BeGreaterThan(0);
        result.Status.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateQuestionSet_WithoutProviderConfig_ShouldReturnNotFound()
    {
        // Arrange — use test1 (Examinee) who has no config saved
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                LoggedInUserDictionary.FirstOrDefault(x => x.Key == "test1@gmail.com").Value);

        // Delete any existing config to ensure clean state
        await HttpClient.DeleteAsync(AIModuleConstants.Route.ProviderConfig.DeleteProviderConfig);

        var command = new GenerateQuestionSetCommand(
            Topics: ["C#"],
            Complexity: "beginner",
            QuestionCount: 10,
            ExperienceYears: null,
            ExpertiseFields: null);

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.Generation.GenerateQuestionSet, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GenerateQuestionSet_WithEmptyTopics_ShouldReturnBadRequest()
    {
        // Arrange
        await EnsureProviderConfigSaved();
        var command = new GenerateQuestionSetCommand(
            Topics: [],
            Complexity: "beginner",
            QuestionCount: 10,
            ExperienceYears: null,
            ExpertiseFields: null);

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.Generation.GenerateQuestionSet, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateQuestionSet_WithInvalidComplexity_ShouldReturnBadRequest()
    {
        // Arrange
        await EnsureProviderConfigSaved();
        var command = new GenerateQuestionSetCommand(
            Topics: ["C#"],
            Complexity: "super-hard",
            QuestionCount: 10,
            ExperienceYears: null,
            ExpertiseFields: null);

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.Generation.GenerateQuestionSet, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateQuestionSet_WithQuestionCountOutOfRange_ShouldReturnBadRequest()
    {
        // Arrange
        await EnsureProviderConfigSaved();
        var commandTooFew = new GenerateQuestionSetCommand(
            Topics: ["C#"],
            Complexity: "beginner",
            QuestionCount: 5,
            ExperienceYears: null,
            ExpertiseFields: null);

        var commandTooMany = new GenerateQuestionSetCommand(
            Topics: ["C#"],
            Complexity: "beginner",
            QuestionCount: 100,
            ExperienceYears: null,
            ExpertiseFields: null);

        // Act
        var responseTooFew = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.Generation.GenerateQuestionSet, commandTooFew);
        var responseTooMany = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.Generation.GenerateQuestionSet, commandTooMany);

        // Assert
        responseTooFew.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        responseTooMany.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateQuestionSet_ExpertWithoutExperienceYears_ShouldReturnBadRequest()
    {
        // Arrange
        await EnsureProviderConfigSaved();
        var command = new GenerateQuestionSetCommand(
            Topics: ["System Design"],
            Complexity: "expert",
            QuestionCount: 10,
            ExperienceYears: null,
            ExpertiseFields: null);

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.Generation.GenerateQuestionSet, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GenerateFromJobDescription Tests

    [Fact]
    public async Task GenerateFromJobDescription_QuestionSet_ShouldReturnOk()
    {
        // Arrange
        await EnsureProviderConfigSaved();
        var command = new GenerateFromJobDescriptionCommand(
            JobTitle: "Senior .NET Developer",
            JobDescription: ValidJobDescription,
            OutputType: "question_set",
            QuestionCount: 10);

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.Generation.GenerateFromJobDescription, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GenerateFromJobDescriptionResponse>();
        result.Should().NotBeNull();
        result!.GenerationRequestId.Should().NotBeEmpty();
        result.OutputType.Should().Be("question_set");
        result.Title.Should().NotBeNullOrEmpty();
        result.Status.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateFromJobDescription_InterviewPrep_ShouldReturnOk()
    {
        // Arrange
        await EnsureProviderConfigSaved();
        var command = new GenerateFromJobDescriptionCommand(
            JobTitle: "Frontend React Developer",
            JobDescription: ValidJobDescription,
            OutputType: "interview_prep",
            QuestionCount: 0);

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.Generation.GenerateFromJobDescription, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GenerateFromJobDescriptionResponse>();
        result.Should().NotBeNull();
        result!.GenerationRequestId.Should().NotBeEmpty();
        result.OutputType.Should().Be("interview_prep");
        result.Title.Should().NotBeNullOrEmpty();
        result.Status.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateFromJobDescription_WithEmptyJobTitle_ShouldReturnBadRequest()
    {
        // Arrange
        await EnsureProviderConfigSaved();
        var command = new GenerateFromJobDescriptionCommand(
            JobTitle: "",
            JobDescription: ValidJobDescription,
            OutputType: "question_set",
            QuestionCount: 10);

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.Generation.GenerateFromJobDescription, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateFromJobDescription_WithShortJobDescription_ShouldReturnBadRequest()
    {
        // Arrange
        await EnsureProviderConfigSaved();
        var command = new GenerateFromJobDescriptionCommand(
            JobTitle: "Developer",
            JobDescription: "Too short description",
            OutputType: "question_set",
            QuestionCount: 10);

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.Generation.GenerateFromJobDescription, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateFromJobDescription_WithInvalidOutputType_ShouldReturnBadRequest()
    {
        // Arrange
        await EnsureProviderConfigSaved();
        var command = new GenerateFromJobDescriptionCommand(
            JobTitle: "Developer",
            JobDescription: ValidJobDescription,
            OutputType: "invalid_type",
            QuestionCount: 10);

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.Generation.GenerateFromJobDescription, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GenerationHistory Tests

    [Fact]
    public async Task GetGenerationHistory_AfterGeneration_ShouldReturnHistory()
    {
        // Arrange — save config and generate a question set
        await EnsureProviderConfigSaved();
        var generateCmd = new GenerateQuestionSetCommand(
            Topics: ["Docker", "Kubernetes"],
            Complexity: "intermediate",
            QuestionCount: 10,
            ExperienceYears: null,
            ExpertiseFields: null);
        await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.Generation.GenerateQuestionSet, generateCmd);

        // Act
        var response = await HttpClient.GetAsync(
            $"{AIModuleConstants.Route.Generation.GetGenerationHistory}?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.Content.ReadFromJsonAsync<List<GenerationHistoryItemDto>>();
        history.Should().NotBeNull();
        history.Should().NotBeEmpty();
        history!.First().Id.Should().NotBeEmpty();
        history.First().Status.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetGenerationHistory_WithNoHistory_ShouldReturnEmptyList()
    {
        // Arrange — use test3 (Examinee) who has no generation history
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                LoggedInUserDictionary.FirstOrDefault(x => x.Key == "test3@gmail.com").Value);

        // Act
        var response = await HttpClient.GetAsync(
            $"{AIModuleConstants.Route.Generation.GetGenerationHistory}?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.Content.ReadFromJsonAsync<List<GenerationHistoryItemDto>>();
        history.Should().NotBeNull();
        history.Should().BeEmpty();
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task GenerationEndpoints_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange — clear auth header
        HttpClient.DefaultRequestHeaders.Authorization = null;

        var generateCmd = new GenerateQuestionSetCommand(
            Topics: ["C#"],
            Complexity: "beginner",
            QuestionCount: 10,
            ExperienceYears: null,
            ExpertiseFields: null);

        var jdCmd = new GenerateFromJobDescriptionCommand(
            JobTitle: "Developer",
            JobDescription: ValidJobDescription,
            OutputType: "question_set",
            QuestionCount: 10);

        // Act
        var generateResponse = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.Generation.GenerateQuestionSet, generateCmd);
        var jdResponse = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.Generation.GenerateFromJobDescription, jdCmd);
        var historyResponse = await HttpClient.GetAsync(
            $"{AIModuleConstants.Route.Generation.GetGenerationHistory}?pageNumber=1&pageSize=10");

        // Assert
        generateResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        jdResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        historyResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
