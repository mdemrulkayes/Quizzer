using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.AI.Core.Providers;
using Modules.AI.Core.Repositories;
using Modules.AI.Core.Security;
using Modules.AI.Infrastructure.Data;
using Modules.Exam.Infrastructure.Persistence;
using Modules.Identity.Persistence;
using Modules.Quiz.Infrastructure.Data;
using Quizzer.Api.FunctionalTest.Mocks;
using Shared.Core.Caching;
using StackExchange.Redis;
using Testcontainers.MsSql;

namespace Quizzer.Api.FunctionalTest.Abstraction;
public class QuizzerWebApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{

    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Pass@word")
        .Build();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var cs = new SqlConnectionStringBuilder(_msSqlContainer.GetConnectionString())
        {
            InitialCatalog = "Quizzer"
        }.ToString();
        builder.ConfigureServices(services =>
        {
            var descriptorType =
                typeof(DbContextOptions<IdentityModuleDbContext>);

            var questionModuleDbContextDescriptor = typeof(DbContextOptions<QuestionModuleDbContext>);

            var examModuleDbContextDescriptor = typeof(DbContextOptions<ExamModuleDbContext>);

            var descriptor = services
                .Where(s => s.ServiceType == descriptorType
                    || s.ServiceType == questionModuleDbContextDescriptor
                    || s.ServiceType == examModuleDbContextDescriptor)
                .ToList();

            if (descriptor.Any())
            {
                foreach (var serviceDescriptor in descriptor)
                {
                    services.Remove(serviceDescriptor);
                }
            }
            services.AddDbContext<IdentityModuleDbContext>(opt =>
            {
                opt.UseSqlServer(cs);
            });

            services.AddDbContext<QuestionModuleDbContext>(opt =>
            {
                opt.UseSqlServer(cs);
            });

            services.AddDbContext<ExamModuleDbContext>(opt =>
            {
                opt.UseSqlServer(cs);
            });

            // Replace Redis cache with in-memory no-op for test isolation
            var cacheDescriptors = services
                .Where(s => s.ServiceType == typeof(ICacheService)
                    || s.ServiceType == typeof(IConnectionMultiplexer))
                .ToList();
            foreach (var d in cacheDescriptors)
                services.Remove(d);

            services.AddSingleton<ICacheService, NoOpCacheService>();
            services.AddDistributedMemoryCache();

            // Replace MongoDB-backed AI module services with in-memory mocks
            var mongoDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(AIModuleMongoContext));
            if (mongoDescriptor is not null)
                services.Remove(mongoDescriptor);

            services.AddSingleton<IAIProviderConfigRepository, InMemoryAIProviderConfigRepository>();
            services.AddSingleton<IAIGenerationRequestRepository, InMemoryAIGenerationRequestRepository>();
            services.AddSingleton<IInterviewPrepMaterialRepository, InMemoryInterviewPrepMaterialRepository>();
            services.AddSingleton<IApiKeyEncryptionService, MockApiKeyEncryptionService>();
            services.AddSingleton<IAIProviderFactory, MockAIProviderFactory>();
        });
    }

    public Task InitializeAsync()
    {
        return _msSqlContainer.StartAsync();
    }

    public new Task DisposeAsync()
    {
        _msSqlContainer.StopAsync();
        return _msSqlContainer.DisposeAsync().AsTask();
    }
}
