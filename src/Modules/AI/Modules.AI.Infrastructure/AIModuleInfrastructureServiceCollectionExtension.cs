using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.AI.Core.Providers;
using Modules.AI.Core.Repositories;
using Modules.AI.Core.Security;
using Modules.AI.Infrastructure.Data;
using Modules.AI.Infrastructure.Persistence;
using Modules.AI.Infrastructure.Providers;
using Modules.AI.Infrastructure.Security;
using Serilog;
using System.Reflection;
using Polly;

namespace Modules.AI.Infrastructure;

public static class AIModuleInfrastructureServiceCollectionExtension
{
    public static IServiceCollection RegisterAIModuleInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger,
        List<Assembly> mediatRAssemblies)
    {
        var connectionString = configuration.GetConnectionString("AIModuleMongoDb")
            ?? "mongodb://localhost:27017";
        var databaseName = "QuizzerAI";

        services.AddSingleton<AIModuleMongoContext>(sp =>
            new AIModuleMongoContext(connectionString, databaseName));

        // Repositories
        services.AddScoped<IAIProviderConfigRepository, AIProviderConfigRepository>();
        services.AddScoped<IAIGenerationRequestRepository, AIGenerationRequestRepository>();
        services.AddScoped<IInterviewPrepMaterialRepository, InterviewPrepMaterialRepository>();

        // Security
        services.AddScoped<IApiKeyEncryptionService, ApiKeyEncryptionService>();

        // AI Providers
        services.AddHttpClient("Gemini")
            .AddStandardResilienceHandler(options =>
            {
                // Configure retry logic
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(2);
                options.Retry.BackoffType = DelayBackoffType.Exponential;

                // Ensure it handles 429 status codes
                options.Retry.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(res => res.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    .Handle<HttpRequestException>();
            });
        services.AddHttpClient("Groq")
            .AddStandardResilienceHandler(options =>
            {
                // Configure retry logic
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(2);
                options.Retry.BackoffType = DelayBackoffType.Exponential;

                // Ensure it handles 429 status codes
                options.Retry.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(res => res.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    .Handle<HttpRequestException>();
            }); ;
        services.AddScoped<GeminiProvider>();
        services.AddScoped<GroqProvider>();
        services.AddScoped<IAIProviderFactory, AIProviderFactory>();

        logger.Information("AI module infrastructure registered");

        return services;
    }
}
