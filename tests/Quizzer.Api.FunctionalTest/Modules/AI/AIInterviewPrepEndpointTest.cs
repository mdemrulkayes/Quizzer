using FluentAssertions;
using Modules.AI.Application.Dtos;
using Modules.AI.Application.Generation.Commands.GenerateFromJobDescription;
using Modules.AI.Application.ProviderConfig.Commands.SaveProviderConfig;
using Modules.AI.Core;
using Quizzer.Api.FunctionalTest.Abstraction;
using System.Net;
using System.Net.Http.Json;

namespace Quizzer.Api.FunctionalTest.Modules.AI;

public class AIInterviewPrepEndpointTest : QuizzerBaseFunctionTest
{
    public AIInterviewPrepEndpointTest(QuizzerWebApiFactory factory) : base(factory)
    {
        RegisterOneTimeUser().Wait();
        LoginOneTimeUser().Wait();
    }

    [Fact]
    public async Task GetInterviewPrepMaterials_AfterGeneration_ShouldReturnList()
    {
        // Arrange
        await SetupProviderAndGenerateInterviewPrep();

        // Act
        var response = await HttpClient.GetAsync(
            $"{AIModuleConstants.Route.InterviewPrep.GetAll}?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var materials = await response.Content.ReadFromJsonAsync<List<InterviewPrepMaterialDto>>();
        materials.Should().NotBeNull();
        materials.Should().NotBeEmpty();
        materials!.Should().Contain(m => m.JobTitle == "Senior Software Engineer");
    }

    [Fact]
    public async Task GetInterviewPrepMaterials_WithNoMaterials_ShouldReturnEmptyList()
    {
        // Arrange — use test3 (Examinee) who has no materials generated
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                LoggedInUserDictionary.FirstOrDefault(x => x.Key == "test3@gmail.com").Value);

        // Act
        var response = await HttpClient.GetAsync(
            $"{AIModuleConstants.Route.InterviewPrep.GetAll}?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var materials = await response.Content.ReadFromJsonAsync<List<InterviewPrepMaterialDto>>();
        materials.Should().NotBeNull();
        materials.Should().BeEmpty();
    }

    [Fact]
    public async Task GetInterviewPrepMaterialById_ShouldReturnDetail()
    {
        // Arrange
        await SetupProviderAndGenerateInterviewPrep();

        var listResponse = await HttpClient.GetAsync(
            $"{AIModuleConstants.Route.InterviewPrep.GetAll}?pageNumber=1&pageSize=10");
        var materials = await listResponse.Content.ReadFromJsonAsync<List<InterviewPrepMaterialDto>>();
        var materialId = materials!.First(m => m.JobTitle == "Senior Software Engineer").Id;

        // Act
        var response = await HttpClient.GetAsync($"/api/ai/interview-prep/{materialId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await response.Content.ReadFromJsonAsync<InterviewPrepMaterialDetailDto>();
        detail.Should().NotBeNull();
        detail!.JobTitle.Should().Be("Senior Software Engineer");
        detail.KeyTopics.Should().HaveCount(5);
        detail.ReadingMaterials.Should().HaveCount(2);
        detail.PracticeQuestions.Should().HaveCount(3);
        detail.PreparationTips.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetInterviewPrepMaterialById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        AddTokenToEachRequest();
        var randomId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/ai/interview-prep/{randomId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task InterviewPrepEndpoints_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange — no token set
        HttpClient.DefaultRequestHeaders.Authorization = null;

        // Act
        var getAllResponse = await HttpClient.GetAsync(
            $"{AIModuleConstants.Route.InterviewPrep.GetAll}?pageNumber=1&pageSize=10");
        var getByIdResponse = await HttpClient.GetAsync($"/api/ai/interview-prep/{Guid.NewGuid()}");

        // Assert
        getAllResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task SetupProviderAndGenerateInterviewPrep()
    {
        AddTokenToEachRequest();
        var configCmd = new SaveProviderConfigCommand("gemini", "test-api-key-that-is-long-enough");
        await HttpClient.PostAsJsonAsync(AIModuleConstants.Route.ProviderConfig.SaveProviderConfig, configCmd);

        var generateCmd = new GenerateFromJobDescriptionCommand(
            "Senior Software Engineer",
            "We are looking for a senior software engineer with experience in .NET, microservices, and cloud architecture to join our growing team.",
            "interview_prep",
            0);
        await HttpClient.PostAsJsonAsync(AIModuleConstants.Route.Generation.GenerateFromJobDescription, generateCmd);
    }
}
