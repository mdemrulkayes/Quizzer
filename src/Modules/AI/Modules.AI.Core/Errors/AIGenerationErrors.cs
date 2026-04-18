using Shared.Core;

namespace Modules.AI.Core.Errors;

public struct AIGenerationErrors
{
    public static readonly Error InvalidResponse = Error.Failure(
        "AIGeneration.InvalidResponse",
        "The AI provider returned an invalid response. Please try again.");

    public static readonly Error GenerationFailed = Error.Failure(
        "AIGeneration.GenerationFailed",
        "Failed to generate content. Please try again later.");

    public static readonly Error InvalidQuestionCount = Error.Validation(
        "AIGeneration.InvalidQuestionCount",
        "Number of questions must be between 10 and 50.");

    public static readonly Error NoTopicsProvided = Error.Validation(
        "AIGeneration.NoTopicsProvided",
        "At least one topic must be provided.");

    public static readonly Error InterviewPrepNotFound = Error.NotFound(
        "AIGeneration.InterviewPrepNotFound",
        "The requested interview preparation material was not found.");
}
