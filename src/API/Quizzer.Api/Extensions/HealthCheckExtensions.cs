using Modules.Exam.Infrastructure.Persistence;
using Modules.Identity.Persistence;
using Modules.Quiz.Infrastructure.Data;

namespace Quizzer.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<IdentityModuleDbContext>("identity-db")
            .AddDbContextCheck<QuestionModuleDbContext>("question-db")
            .AddDbContextCheck<ExamModuleDbContext>("exam-db");

        return services;
    }

    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        return app;
    }
}
