using Modules.Exam.Infrastructure.Persistence;
using Modules.Identity.Persistence;
using Modules.Quiz.Infrastructure.Data;

namespace Quizzer.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<IdentityModuleDbContext>("identity-db")
            .AddDbContextCheck<QuestionModuleDbContext>("question-db")
            .AddDbContextCheck<ExamModuleDbContext>("exam-db")
            .AddRedis(
                configuration.GetConnectionString("Redis") ?? "localhost:6379",
                name: "redis");

        return services;
    }

    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        return app;
    }
}
