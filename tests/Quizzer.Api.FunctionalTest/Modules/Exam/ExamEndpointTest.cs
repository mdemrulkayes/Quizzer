using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Modules.Exam.Application.Features.ExamManagement.Create;
using Modules.Exam.Application.Features.ExamManagement.Dtos;
using Modules.Exam.Core.ExamAggregate;
using Modules.Identity.Constants;
using Modules.Identity.Features.Login;
using Quizzer.Api.FunctionalTest.Abstraction;

namespace Quizzer.Api.FunctionalTest.Modules.Exam;

public class ExamEndpointTest : QuizzerBaseFunctionTest
{
    public ExamEndpointTest(QuizzerWebApiFactory factory) : base(factory)
    {
        RegisterOneTimeUser().Wait();
    }

    private async Task<string> GetQuizAuthorToken()
    {
        var loginResponse = await HttpClient.PostAsJsonAsync(
            IdentityModuleConstants.Route.Login,
            new LoginCommand("test2@gmail.com", "Aa123456!"));
        var content = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        return content!.Token;
    }

    private async Task<string> GetExamineToken()
    {
        var loginResponse = await HttpClient.PostAsJsonAsync(
            IdentityModuleConstants.Route.Login,
            new LoginCommand("test1@gmail.com", "Aa123456#"));
        var content = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        return content!.Token;
    }

    [Fact]
    public async Task Should_CreateExam_WhenQuizAuthorCreatesExam()
    {
        // Arrange: Create a question set first
        var quizAuthorToken = await GetQuizAuthorToken();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", quizAuthorToken);

        var createSetResponse = await HttpClient.PostAsJsonAsync(
            "/api/question/questionSet",
            new { Name = "ExamSet", SetCode = "ES01", Details = "Set for exam testing" });
        createSetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act: Create an exam
        var createExamResponse = await HttpClient.PostAsJsonAsync(
            ExamModuleConstants.Route.CreateExam,
            new CreateExamCommand("Test Exam", "A test exam", 1, 60, 100, 50, null, null));

        // Assert
        createExamResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var exam = await createExamResponse.Content.ReadFromJsonAsync<ExamResponse>();
        exam.Should().NotBeNull();
        exam!.Title.Should().Be("Test Exam");
        exam.DurationInMinutes.Should().Be(60);
        exam.TotalMarks.Should().Be(100);
    }

    [Fact]
    public async Task Should_ReturnForbidden_WhenExamineTriesToCreateExam()
    {
        // Arrange
        var examineToken = await GetExamineToken();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", examineToken);

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            ExamModuleConstants.Route.CreateExam,
            new CreateExamCommand("Test", "Desc", 1, 30, 50, 25, null, null));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Should_ReturnForbidden_WhenQuizAuthorTriesToStartExam()
    {
        // Arrange
        var quizAuthorToken = await GetQuizAuthorToken();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", quizAuthorToken);

        // Act: QuizAuthor should not be able to start an exam (Examine role required)
        var response = await HttpClient.PostAsJsonAsync(
            "/api/exam/1/start", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_WhenNoTokenProvided()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await HttpClient.GetAsync(ExamModuleConstants.Route.GetAllExams);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_GetAvailableExams_WhenExamineIsAuthenticated()
    {
        // Arrange
        var examineToken = await GetExamineToken();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", examineToken);

        // Act
        var response = await HttpClient.GetAsync(ExamModuleConstants.Route.GetAvailableExams);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
