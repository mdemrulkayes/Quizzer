using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Modules.AI.Core.Providers;
using Shared.Core;

namespace Modules.AI.Infrastructure.Providers;

public class GroqProvider(IHttpClientFactory httpClientFactory, ILogger<GroqProvider> logger) : IAIProvider
{
    private const string BaseUrl = "https://api.groq.com/openai/v1/chat/completions";

    public string ProviderId => "groq";

    public async Task<Result<string>> GenerateAsync(
        string systemPrompt, string userPrompt, string decryptedApiKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = httpClientFactory.CreateClient("Groq");
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", decryptedApiKey);

            var requestBody = new
            {
                model = "llama3-8b-8192",
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                response_format = new { type = "json_object" },
                temperature = 0.7
            };

            logger.LogDebug("Sending request to Groq API");

            var response = await client.PostAsJsonAsync(BaseUrl, requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Groq API returned {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
                return Error.Failure("AIProvider.ApiError", $"Groq API returned {(int)response.StatusCode}: {errorBody}");
            }

            var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            var content = responseJson
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            logger.LogDebug("Groq API response received successfully");

            if (string.IsNullOrWhiteSpace(content))
            {
                return Error.Failure("AIProvider.EmptyResponse", "Groq API returned an empty response.");
            }

            return content;
        }
        catch (TaskCanceledException)
        {
            logger.LogError("Request to {Provider} timed out", ProviderId);
            return Error.Failure("AIProvider.Timeout", "Request timed out.");
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to parse {Provider} response", ProviderId);
            return Error.Failure("AIProvider.ParseError", $"Failed to parse response: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Request to {Provider} failed", ProviderId);
            return Error.Failure("AIProvider.RequestFailed", $"Request failed: {ex.Message}");
        }
    }

    public async Task<Result<bool>> TestConnectionAsync(
        string decryptedApiKey, CancellationToken cancellationToken = default)
    {
        var result = await GenerateAsync(
            "You are a connection test. Respond only with valid JSON.",
            "Respond with: {\"status\":\"ok\"}",
            decryptedApiKey,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error;
        }

        return true;
    }
}
