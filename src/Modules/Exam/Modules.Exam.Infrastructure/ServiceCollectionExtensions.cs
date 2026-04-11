using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Exam.Core.ExamAggregate;
using Modules.Exam.Infrastructure.Persistence;
using Modules.Exam.Infrastructure.Persistence.Repositories;
using Serilog;
using Shared.Core;
using Shared.Infrastructure.Interceptors;

namespace Modules.Exam.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterExamModule(this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger,
        List<Assembly> mediatRAssembly)
    {
        mediatRAssembly.Add(typeof(ServiceCollectionExtensions).Assembly);

        services.AddScoped<PopulateAuditableEntityInterceptor>();
        services.AddDbContext<ExamModuleDbContext>((serviceProvider, opt) =>
        {
            opt.UseSqlServer(configuration.GetConnectionString("ExamModuleDbContext"), optBuilder =>
            {
                optBuilder.EnableRetryOnFailure(10);
                optBuilder.MigrationsHistoryTable(ExamModuleConstants.MigrationHistoryTableName,
                    ExamModuleConstants.SchemaName);
                optBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            }).AddInterceptors(serviceProvider.GetRequiredService<PopulateAuditableEntityInterceptor>());
        });
        logger.Information("{Module} registered successfully", "Exam");

        RegisterRepositories(services);

        return services;
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IExamRepository, ExamRepository>();
        services.AddScoped<IExamAttemptRepository, ExamAttemptRepository>();
    }

    public static IApplicationBuilder MigrateExamModuleDatabase(this IApplicationBuilder app)
    {
        var scopedService = app.ApplicationServices.CreateScope();
        var dbContext = scopedService.ServiceProvider.GetRequiredService<ExamModuleDbContext>();

        if (dbContext.Database.IsSqlServer())
        {
            dbContext.Database.Migrate();
        }

        return app;
    }
}
