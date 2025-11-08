using FluentAssertions;
using Modules.Quiz.Application.Question.Question.Create;
using Modules.Quiz.Application.Question.QuestionSet.Create;
using Modules.Quiz.Application.Question.QuestionSet.Dtos;
using Modules.Quiz.Core;
using Quizzer.Api.FunctionalTest.Abstraction;
using System.Net;
using System.Net.Http.Json;

namespace Quizzer.Api.FunctionalTest.Modules.Question;

public class QuestionSetEndPointTest : QuizzerBaseFunctionTest
{
    public QuestionSetEndPointTest(QuizzerWebApiFactory factory) : base(factory)
    {
        RegisterOneTimeUser().Wait();
        LoginOneTimeUser().Wait();
    }

    [Theory]
    [InlineData("Sample Question Set", "S001", "This is a sample question set description.")]
    public async Task Should_CreateQuestionSetSuccessfully(string title, string setCode, string description)
    {
        // Arrange
        AddTokenToEachRequest();
        var questionCommands = new List<CreateQuestionCommand>
        {
            new("Question Number one", "Details will be added here", 5,
            [
                new("Option A", false),
                new("Option B", true),
                new("Option C", false),
                new("Option D", false),
            ]),
        };
        var createQuestionSetCommand = new CreateQuestionSetCommand(title, setCode, description, questionCommands);
        // Act
        var response = await HttpClient.PostAsJsonAsync(QuestionModuleConstants.Route.QuestionSetRoute.CreateQuestionSet, createQuestionSetCommand);
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdQuestionSet = await response.Content.ReadFromJsonAsync<QuestionSetResponse>();
        createdQuestionSet!.Name.Should().Be(title);
        createdQuestionSet.Details.Should().Be(description);
        createdQuestionSet.Questions.Should().HaveCount(1);
        createdQuestionSet.Questions.FirstOrDefault()!.QuestionOptions.Should().HaveCount(4);
    }


}
