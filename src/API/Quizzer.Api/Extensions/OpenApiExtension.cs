using Scalar.AspNetCore;

namespace Quizzer.Api.Extensions;

/// <summary>
/// Extension methods for OpenAPI and Scalar API documentation configuration
/// </summary>
public static class OpenApiExtension
{
    /// <summary>
    /// Register OpenAPI documentation with Scalar UI for .NET 10
    /// Native OpenAPI support replaces deprecated Swagger/Swashbuckle
    /// </summary>
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        // Add native OpenAPI support
        services.AddOpenApi();
        
        return services;
    }

    /// <summary>
    /// Map OpenAPI endpoints and Scalar UI
    /// </summary>
    public static WebApplication MapOpenApiDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // Enable OpenAPI specification endpoint at /openapi/v1.json
            app.MapOpenApi();

            // Map Scalar API Reference UI at /scalar/v1
            app.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("Quizzer API Documentation")
                    .WithTheme(ScalarTheme.Kepler)
                    .WithDarkModeToggle(true);
            });
        }

        return app;
    }
}
