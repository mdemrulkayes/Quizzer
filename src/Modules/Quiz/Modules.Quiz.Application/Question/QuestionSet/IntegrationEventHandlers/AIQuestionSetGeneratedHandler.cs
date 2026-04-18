using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Quiz.Core.Enums;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Core;
using Shared.Core.IntegrationEvents;
using Shared.Core.IntegrationEvents.Events;

namespace Modules.Quiz.Application.Question.QuestionSet.IntegrationEventHandlers;

internal sealed class AIQuestionSetGeneratedHandler(
    IQuestionSetRepository repository,
    [FromKeyedServices(ModuleKeys.Quiz)] IUnitOfWork unitOfWork,
    ILogger<AIQuestionSetGeneratedHandler> logger)
    : IIntegrationEventHandler<AIQuestionSetGeneratedEvent>
{
    public async Task HandleAsync(AIQuestionSetGeneratedEvent @event, CancellationToken cancellationToken = default)
    {
        var source = @event.Source switch
        {
            "topic" => QuestionSetSource.AITopic,
            "job_description" => QuestionSetSource.AIJobDescription,
            _ => QuestionSetSource.Manual
        };

        var complexity = @event.Complexity switch
        {
            "beginner" => (Complexity?)Complexity.Beginner,
            "intermediate" => (Complexity?)Complexity.Intermediate,
            "professional" => (Complexity?)Complexity.Professional,
            "expert" => (Complexity?)Complexity.Expert,
            _ => null
        };

        var questionSetResult = Core.QuestionAggregate.QuestionSet.Create(
            name: @event.Title,
            setCode: null,
            details: null,
            source: source,
            isPublic: @event.IsPublic,
            complexity: complexity,
            experienceYears: @event.ExperienceYears,
            expertiseFields: @event.ExpertiseFields);

        if (!questionSetResult.IsSuccess || questionSetResult.Value is null)
        {
            logger.LogError("Failed to create question set from AI generation: {Error}", questionSetResult.Error);
            return;
        }

        var set = questionSetResult.Value;

        var questions = new List<Core.QuestionAggregate.Question>();

        foreach (var q in @event.Questions)
        {
            var questionType = q.Type switch
            {
                "true_false" => QuestionType.TrueFalse,
                "short_answer" => QuestionType.ShortAnswer,
                _ => QuestionType.MultipleChoice
            };

            var questionResult = Core.QuestionAggregate.Question.Create(
                askedQuestion: q.Text,
                discussion: "",
                mark: null,
                questionType: questionType,
                explanation: q.Explanation,
                difficultyScore: q.DifficultyScore,
                sequence: q.Sequence);

            if (questionResult.Value is null) continue;

            var question = questionResult.Value;

            foreach (var opt in q.Options)
            {
                var isCorrect = opt.Id == q.CorrectOptionId;
                question.AddQuestionOptions(opt.Text, isCorrect, opt.Id);
            }

            questions.Add(question);
        }

        set.AddQuestions(questions);

        repository.Add(set);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Created AI-generated question set '{Title}' with {QuestionCount} questions",
            @event.Title, @event.Questions.Count);
    }
}
