﻿using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Question.Infrastructure.Data;
using Serilog;
using System.Reflection;
using Modules.Question.Core.QuestionAggregate;
using Modules.Question.Core.Tag;
using Modules.Question.Infrastructure.Persistence;
using SharedKernel.Core;
using SharedKernel.Infrastructure.Interceptors;
using Modules.Question.Core;

namespace Modules.Question.Infrastructure;
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
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IQuestionSetRepository, QuestionSetRepository>();
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
