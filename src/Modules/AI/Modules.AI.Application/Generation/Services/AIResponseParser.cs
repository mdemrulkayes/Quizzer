using System.Text.Json;
using System.Text.Json.Serialization;
using Modules.AI.Core.Errors;
using Shared.Core;

namespace Modules.AI.Application.Generation.Services;

public static class AIResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static Result<QuestionSetAIResponse> ParseQuestionSetResponse(string json)
    {
        try
        {
            json = CleanJsonResponse(json);

            var result = JsonSerializer.Deserialize<QuestionSetAIResponse>(json, JsonOptions);
            if (result is null)
                return AIGenerationErrors.InvalidResponse;

            if (result.Questions is null || result.Questions.Count == 0)
                return AIGenerationErrors.InvalidResponse;

            if (result.Questions.Count < 10 || result.Questions.Count > 50)
                return AIGenerationErrors.InvalidQuestionCount;

            return result;
        }
        catch (JsonException)
        {
            return AIGenerationErrors.InvalidResponse;
        }
    }

    public static Result<InterviewPrepAIResponse> ParseInterviewPrepResponse(string json)
    {
        try
        {
            json = CleanJsonResponse(json);

            var result = JsonSerializer.Deserialize<InterviewPrepAIResponse>(json, JsonOptions);
            if (result is null)
                return AIGenerationErrors.InvalidResponse;

            if (string.IsNullOrWhiteSpace(result.JobTitle))
                return AIGenerationErrors.InvalidResponse;

            return result;
        }
        catch (JsonException)
        {
            return AIGenerationErrors.InvalidResponse;
        }
    }

    private static string CleanJsonResponse(string json)
    {
        json = json.Trim();

        if (json.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            json = json[7..];
        else if (json.StartsWith("```"))
            json = json[3..];

        if (json.EndsWith("```"))
            json = json[..^3];

        return json.Trim();
    }
}

public sealed class QuestionSetAIResponse
{
    public string Title { get; set; } = default!;
    public string Source { get; set; } = default!;
    public string Complexity { get; set; } = default!;
    public int? ExperienceYears { get; set; }
    public List<string>? ExpertiseFields { get; set; }
    public List<string>? Topics { get; set; }
    public int TotalQuestions { get; set; }
    public List<QuestionAIResponse> Questions { get; set; } = new();
}

public sealed class QuestionAIResponse
{
    public int Sequence { get; set; }
    public string Text { get; set; } = default!;
    public string Type { get; set; } = default!;
    public List<OptionAIResponse> Options { get; set; } = new();
    public string? CorrectOptionId { get; set; }
    public string? Explanation { get; set; }
    public List<string>? Tags { get; set; }
    public int DifficultyScore { get; set; }
}

public sealed class OptionAIResponse
{
    public string Id { get; set; } = default!;
    public string Text { get; set; } = default!;
}

public sealed class InterviewPrepAIResponse
{
    public string JobTitle { get; set; } = default!;
    public List<string> KeyTopics { get; set; } = new();
    public List<ReadingMaterialAIResponse> ReadingMaterials { get; set; } = new();
    public List<PracticeQuestionAIResponse> PracticeQuestions { get; set; } = new();
    public List<string> PreparationTips { get; set; } = new();
}

public sealed class ReadingMaterialAIResponse
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Url { get; set; }
    public string Type { get; set; } = default!;
}

public sealed class PracticeQuestionAIResponse
{
    public string Question { get; set; } = default!;
    public string Hint { get; set; } = default!;
}
