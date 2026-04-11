using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Exam.Infrastructure.Persistence;
using Modules.Identity.Persistence;
using Modules.Quiz.Infrastructure.Data;
using Testcontainers.MsSql;

namespace Quizzer.Api.FunctionalTest.Abstraction;
public class QuizzerWebApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{

    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
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
