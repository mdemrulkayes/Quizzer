using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Modules.AI.Core.Errors;
using Modules.AI.Core.Providers;
using Shared.Core;

namespace Modules.AI.Infrastructure.Providers;

public class GeminiProvider(IHttpClientFactory httpClientFactory, ILogger<GeminiProvider> logger) : IAIProvider
{
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    public string ProviderId => "gemini";

    public async Task<Result<string>> GenerateAsync(
        string systemPrompt, string userPrompt, string decryptedApiKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = httpClientFactory.CreateClient("Gemini");
            client.Timeout = TimeSpan.FromSeconds(60);

            var combinedPrompt = $"{systemPrompt}\n\n{userPrompt}";
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = combinedPrompt } }
                    }
                },
                generationConfig = new
                {
                    responseMimeType = "application/json",
                    temperature = 0.7
                }
            };

            var requestUrl = $"{BaseUrl}?key={decryptedApiKey}";

            logger.LogDebug("Sending request to Gemini API");

            var response = await client.PostAsJsonAsync(requestUrl, requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Gemini API returned {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
                return ParseProviderError(response.StatusCode, errorBody);
            }

            var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            var content = responseJson
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            logger.LogDebug("Gemini API response received successfully");

            if (string.IsNullOrWhiteSpace(content))
            {
                return Error.Failure("AIProvider.EmptyResponse", "Gemini API returned an empty response.");
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

    private static Error ParseProviderError(HttpStatusCode statusCode, string errorBody)
    {
        var message = TryExtractErrorMessage(errorBody);

        return (int)statusCode switch
        {
            401 => AIProviderErrors.AuthenticationFailed(
                message ?? "Authentication failed. Please check your API key."),
            403 => AIProviderErrors.AuthenticationFailed(
                message ?? "Access denied. Please check your API key permissions."),
            429 => AIProviderErrors.RateLimitExceeded(
                message ?? "Rate limit exceeded. Please wait before retrying."),
            >= 500 => AIProviderErrors.ServerError(
                message ?? "The Gemini API encountered a server error. Please try again later."),
            _ => AIProviderErrors.ApiError(
                message ?? $"Gemini API returned an error (HTTP {(int)statusCode}).")
        };
    }

    private static string? TryExtractErrorMessage(string errorBody)
    {
        try
        {
            var json = JsonDocument.Parse(errorBody);
            if (json.RootElement.TryGetProperty("error", out var errorObj) &&
                errorObj.TryGetProperty("message", out var msgProp))
            {
                return msgProp.GetString();
            }
        }
        catch
        {
            // ignore parse failures — fall through to null
        }

        return null;
    }
}
