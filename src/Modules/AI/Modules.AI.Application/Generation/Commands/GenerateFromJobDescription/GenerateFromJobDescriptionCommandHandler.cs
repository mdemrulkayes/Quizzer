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

namespace Modules.AI.Application.Generation.Commands.GenerateFromJobDescription;

internal sealed class GenerateFromJobDescriptionCommandHandler(
    IUser user,
    IAIProviderFactory providerFactory,
    IAIGenerationRequestRepository generationRequestRepository,
    IInterviewPrepMaterialRepository interviewPrepMaterialRepository,
    IIntegrationEventPublisher integrationEventPublisher)
    : ICommandHandler<GenerateFromJobDescriptionCommand, Result<GenerateFromJobDescriptionResponse>>
{
    public async Task<Result<GenerateFromJobDescriptionResponse>> Handle(
        GenerateFromJobDescriptionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            return Error.Unauthorized("User.NotAuthenticated", "User is not authenticated.");
        }

        var userId = Guid.Parse(user.Id);

        var resolveResult = await providerFactory.ResolveForCurrentUserAsync(cancellationToken);
        if (!resolveResult.IsSuccess)
            return resolveResult.Error;

        var (provider, decryptedKey) = resolveResult.Value!;

        var outputType = request.OutputType == "question_set"
            ? GenerationOutputType.QuestionSet
            : GenerationOutputType.InterviewPrep;

        var generationRequest = new AIGenerationRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Source = GenerationSource.JobDescription,
            OutputType = outputType,
            Parameters = new Dictionary<string, object>
            {
                ["jobTitle"] = request.JobTitle,
                ["jobDescription"] = request.JobDescription,
                ["outputType"] = request.OutputType
            },
            Status = GenerationStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        if (request.OutputType == "question_set")
            generationRequest.Parameters["questionCount"] = request.QuestionCount;

        await generationRequestRepository.SaveAsync(generationRequest, cancellationToken);

        return request.OutputType == "question_set"
            ? await HandleQuestionSetGeneration(request, provider, decryptedKey, generationRequest, userId, cancellationToken)
            : await HandleInterviewPrepGeneration(request, provider, decryptedKey, generationRequest, userId, cancellationToken);
    }

    private async Task<Result<GenerateFromJobDescriptionResponse>> HandleQuestionSetGeneration(
        GenerateFromJobDescriptionCommand request,
        IAIProvider provider,
        string decryptedKey,
        AIGenerationRequest generationRequest,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var (systemPrompt, userPrompt) = PromptBuilder.BuildJobDescriptionQuestionSetPrompt(
            request.JobTitle,
            request.JobDescription,
            request.QuestionCount);

        var generateResult = await provider.GenerateAsync(systemPrompt, userPrompt, decryptedKey, cancellationToken);
        if (!generateResult.IsSuccess)
        {
            await MarkFailed(generationRequest, generateResult.Error.Message, null, cancellationToken);
            return AIGenerationErrors.GenerationFailed;
        }

        var responseJson = generateResult.Value!;
        var parseResult = AIResponseParser.ParseQuestionSetResponse(responseJson);

        if (!parseResult.IsSuccess)
        {
            var retryPrompt = userPrompt + "\n\nYour previous response was not valid JSON. Return ONLY valid JSON.";
            var retryResult = await provider.GenerateAsync(systemPrompt, retryPrompt, decryptedKey, cancellationToken);
            if (retryResult.IsSuccess)
                parseResult = AIResponseParser.ParseQuestionSetResponse(retryResult.Value!);

            if (!parseResult.IsSuccess)
            {
                await MarkFailed(generationRequest, "Failed to parse AI response after retry.", responseJson, cancellationToken);
                return AIGenerationErrors.InvalidResponse;
            }
        }

        var parsed = parseResult.Value!;

        var integrationEvent = new AIQuestionSetGeneratedEvent
        {
            Title = parsed.Title,
            Source = "job_description",
            Complexity = parsed.Complexity,
            ExperienceYears = parsed.ExperienceYears,
            ExpertiseFields = parsed.ExpertiseFields is { Count: > 0 }
                ? string.Join(", ", parsed.ExpertiseFields)
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

        return new GenerateFromJobDescriptionResponse(
            generationRequest.Id,
            "question_set",
            parsed.Title,
            "completed");
    }

    private async Task<Result<GenerateFromJobDescriptionResponse>> HandleInterviewPrepGeneration(
        GenerateFromJobDescriptionCommand request,
        IAIProvider provider,
        string decryptedKey,
        AIGenerationRequest generationRequest,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var (systemPrompt, userPrompt) = PromptBuilder.BuildInterviewPrepPrompt(
            request.JobTitle,
            request.JobDescription);

        var generateResult = await provider.GenerateAsync(systemPrompt, userPrompt, decryptedKey, cancellationToken);
        if (!generateResult.IsSuccess)
        {
            await MarkFailed(generationRequest, generateResult.Error.Message, null, cancellationToken);
            return AIGenerationErrors.GenerationFailed;
        }

        var responseJson = generateResult.Value!;
        var parseResult = AIResponseParser.ParseInterviewPrepResponse(responseJson);

        if (!parseResult.IsSuccess)
        {
            var retryPrompt = userPrompt + "\n\nYour previous response was not valid JSON. Return ONLY valid JSON.";
            var retryResult = await provider.GenerateAsync(systemPrompt, retryPrompt, decryptedKey, cancellationToken);
            if (retryResult.IsSuccess)
                parseResult = AIResponseParser.ParseInterviewPrepResponse(retryResult.Value!);

            if (!parseResult.IsSuccess)
            {
                await MarkFailed(generationRequest, "Failed to parse AI response after retry.", responseJson, cancellationToken);
                return AIGenerationErrors.InvalidResponse;
            }
        }

        var parsed = parseResult.Value!;

        var interviewPrepMaterial = new InterviewPrepMaterial
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            JobTitle = parsed.JobTitle,
            JobDescription = request.JobDescription,
            KeyTopics = parsed.KeyTopics,
            ReadingMaterials = parsed.ReadingMaterials.Select(r => new ReadingMaterial
            {
                Title = r.Title,
                Description = r.Description,
                Url = r.Url,
                Type = r.Type
            }).ToList(),
            PracticeQuestions = parsed.PracticeQuestions.Select(p => new PracticeQuestion
            {
                Question = p.Question,
                Hint = p.Hint
            }).ToList(),
            PreparationTips = parsed.PreparationTips,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await interviewPrepMaterialRepository.SaveAsync(interviewPrepMaterial, cancellationToken);

        generationRequest.Status = GenerationStatus.Completed;
        generationRequest.RawAIResponse = responseJson;
        generationRequest.ResultInterviewPrepId = interviewPrepMaterial.Id;
        generationRequest.CompletedAt = DateTimeOffset.UtcNow;
        await generationRequestRepository.UpdateAsync(generationRequest, cancellationToken);

        return new GenerateFromJobDescriptionResponse(
            generationRequest.Id,
            "interview_prep",
            parsed.JobTitle,
            "completed");
    }

    private async Task MarkFailed(
        AIGenerationRequest generationRequest,
        string errorMessage,
        string? rawResponse,
        CancellationToken cancellationToken)
    {
        generationRequest.Status = GenerationStatus.Failed;
        generationRequest.ErrorMessage = errorMessage;
        generationRequest.RawAIResponse = rawResponse;
        generationRequest.CompletedAt = DateTimeOffset.UtcNow;
        await generationRequestRepository.UpdateAsync(generationRequest, cancellationToken);
    }
}
