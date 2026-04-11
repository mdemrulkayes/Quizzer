using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Quiz.Core;
using Modules.Quiz.Core.QuestionAggregate;
using Modules.Quiz.Core.Tag;
using Modules.Quiz.Infrastructure.Data;
using Modules.Quiz.Infrastructure.Persistence;
using Serilog;
using Shared.Core;
using Shared.Infrastructure.Interceptors;

namespace Modules.Quiz.Infrastructure;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterQuestionModuleInfrastructure(this IServiceCollection services,
        ILogger logger,
        IConfiguration configuration,
        List<Assembly> mediatRAssembly)
    {
        mediatRAssembly.Add(typeof(ServiceCollectionExtensions).Assembly);

        services.AddScoped<PopulateAuditableEntityInterceptor>();
        services.AddDbContext<QuestionModuleDbContext>((serviceProvider, opt) =>
        {
            opt.UseSqlServer(configuration.GetConnectionString("QuestionModuleDbContext"), optBuilder =>
            {
                optBuilder.EnableRetryOnFailure(10);
                optBuilder.MigrationsHistoryTable(QuestionModuleConstants.MigrationHistoryTableName,
                    QuestionModuleConstants.SchemaName);
                optBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            }).AddInterceptors(serviceProvider.GetRequiredService<PopulateAuditableEntityInterceptor>());
        });
        logger.Information("Question module db context registered");

        services.AddScoped<DbContext, QuestionModuleDbContext>();

        RegisterRepositories(services);

        return services;
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
        services.AddKeyedScoped<IUnitOfWork, UnitOfWork>(ModuleKeys.Quiz);
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IQuestionSetRepository, QuestionSetRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<Shared.Core.ModuleServices.IQuestionQueryService, QuestionQueryService>();
    }

    public static IApplicationBuilder MigrateQuestionModuleDatabase(this IApplicationBuilder app)
    {
        var scopedService = app.ApplicationServices.CreateScope();
        var dbContext = scopedService.ServiceProvider.GetRequiredService<QuestionModuleDbContext>();

        if (dbContext.Database.IsSqlServer())
        {
            dbContext.Database.Migrate();
        }

        return app;
    }
}
