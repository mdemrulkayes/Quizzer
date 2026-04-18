using Modules.AI.Core.Providers;
using Shared.Core;

namespace Quizzer.Api.FunctionalTest.Mocks;

public class MockGeminiProvider : IAIProvider
{
    public string ProviderId => "gemini";

    public Task<Result<string>> GenerateAsync(string systemPrompt, string userPrompt, string decryptedApiKey, CancellationToken cancellationToken = default)
    {
        var json = """
        {
            "title": "Test Question Set",
            "source": "topic",
            "complexity": "beginner",
            "experienceYears": null,
            "expertiseFields": [],
            "topics": ["Testing"],
            "totalQuestions": 1,
            "questions": [
                {
                    "sequence": 1,
                    "text": "What is unit testing?",
                    "type": "multiple_choice",
                    "options": [
                        {"id": "a", "text": "Testing individual units"},
                        {"id": "b", "text": "Testing the whole system"},
                        {"id": "c", "text": "Testing UI only"},
                        {"id": "d", "text": "Testing database only"}
                    ],
                    "correctOptionId": "a",
                    "explanation": "Unit testing tests individual units of code.",
                    "tags": ["testing"],
                    "difficultyScore": 3
                }
            ]
        }
        """;
        Result<string> result = json;
        return Task.FromResult(result);
    }

    public Task<Result<bool>> TestConnectionAsync(string decryptedApiKey, CancellationToken cancellationToken = default)
    {
        Result<bool> result = true;
        return Task.FromResult(result);
    }
}
