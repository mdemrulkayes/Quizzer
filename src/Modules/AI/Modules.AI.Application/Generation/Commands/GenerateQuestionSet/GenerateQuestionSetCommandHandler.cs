using Modules.AI.Application.Dtos;
using Modules.AI.Application.Generation.Services;
using Modules.AI.Core.Enums;
using Modules.AI.Core.Errors;
using Modules.AI.Core.Models;
using Modules.AI.Core.Providers;
using Modules.AI.Core.Repositories;
using Shared.Core;
using Shared.Core.IntegrationEvents;
using Shared.Core.IntegrationEvents.Events;

namespace Modules.AI.Application.Generation.Commands.GenerateQuestionSet;

internal sealed class GenerateQuestionSetCommandHandler(
    IUser user,
    IAIProviderFactory providerFactory,
    IAIGenerationRequestRepository generationRequestRepository,
    IIntegrationEventPublisher integrationEventPublisher)
    : ICommandHandler<GenerateQuestionSetCommand, Result<GenerateQuestionSetResponse>>
{
    public async Task<Result<GenerateQuestionSetResponse>> Handle(
        GenerateQuestionSetCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            return Error.Unauthorized("User.NotAuthenticated", "User is not authenticated.");
        }

        var userId = Guid.Parse(user.Id);

        var (systemPrompt, userPrompt) = PromptBuilder.BuildTopicQuestionSetPrompt(
            request.Topics,
            request.Complexity,
            request.QuestionCount,
            request.ExperienceYears,
            request.ExpertiseFields);

        var resolveResult = await providerFactory.ResolveForCurrentUserAsync(cancellationToken);
        if (!resolveResult.IsSuccess)
            return resolveResult.Error;

        var (provider, decryptedKey) = resolveResult.Value!;

        var generationRequest = new AIGenerationRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Source = GenerationSource.Topic,
            OutputType = GenerationOutputType.QuestionSet,
            Parameters = new Dictionary<string, object>
            {
                ["topics"] = request.Topics,
                ["complexity"] = request.Complexity,
                ["questionCount"] = request.QuestionCount
            },
            Status = GenerationStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        if (request.ExperienceYears.HasValue)
            generationRequest.Parameters["experienceYears"] = request.ExperienceYears.Value;
        if (request.ExpertiseFields is { Count: > 0 })
            generationRequest.Parameters["expertiseFields"] = request.ExpertiseFields;

        await generationRequestRepository.SaveAsync(generationRequest, cancellationToken);

        var generateResult = await provider.GenerateAsync(systemPrompt, userPrompt, decryptedKey, cancellationToken);
        if (!generateResult.IsSuccess)
        {
            generationRequest.Status = GenerationStatus.Failed;
            generationRequest.ErrorMessage = generateResult.Error.Message;
            generationRequest.CompletedAt = DateTimeOffset.UtcNow;
            await generationRequestRepository.UpdateAsync(generationRequest, cancellationToken);
            return AIGenerationErrors.GenerationFailed;
        }

        var responseJson = generateResult.Value!;
        var parseResult = AIResponseParser.ParseQuestionSetResponse(responseJson);

        if (!parseResult.IsSuccess)
        {
            // Retry once with corrective prompt
            var retryPrompt = userPrompt + "\n\nYour previous response was not valid JSON. Return ONLY valid JSON.";
            var retryResult = await provider.GenerateAsync(systemPrompt, retryPrompt, decryptedKey, cancellationToken);
            if (retryResult.IsSuccess)
            {
                parseResult = AIResponseParser.ParseQuestionSetResponse(retryResult.Value!);
            }

            if (!parseResult.IsSuccess)
            {
                generationRequest.Status = GenerationStatus.Failed;
                generationRequest.ErrorMessage = "Failed to parse AI response after retry.";
                generationRequest.RawAIResponse = responseJson;
                generationRequest.CompletedAt = DateTimeOffset.UtcNow;
                await generationRequestRepository.UpdateAsync(generationRequest, cancellationToken);
                return AIGenerationErrors.InvalidResponse;
            }
        }

        var parsed = parseResult.Value!;

        var integrationEvent = new AIQuestionSetGeneratedEvent
        {
            Title = parsed.Title,
            Source = "topic",
            Complexity = request.Complexity,
            ExperienceYears = request.ExperienceYears,
            ExpertiseFields = request.ExpertiseFields is { Count: > 0 }
                ? string.Join(", ", request.ExpertiseFields)
                : null,
            IsPublic = false,
            CreatedByUserId = userId,
            Questions = parsed.Questions.Select(q => new GeneratedQuestionData
            {
                Sequence = q.Sequence,
                Text = q.Text,
                Type = q.Type,
                Options = q.Options.Select(o => new GeneratedOptionData
                {
                    Id = o.Id,
                    Text = o.Text
                }).ToList(),
                CorrectOptionId = q.CorrectOptionId,
                Explanation = q.Explanation,
                Tags = q.Tags ?? [],
                DifficultyScore = q.DifficultyScore
            }).ToList()
        };

        await integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken);

        generationRequest.Status = GenerationStatus.Completed;
        generationRequest.RawAIResponse = responseJson;
        generationRequest.CompletedAt = DateTimeOffset.UtcNow;
        await generationRequestRepository.UpdateAsync(generationRequest, cancellationToken);

        return new GenerateQuestionSetResponse(
            generationRequest.Id,
            parsed.Title,
            parsed.Questions.Count,
            "completed");
    }
}
