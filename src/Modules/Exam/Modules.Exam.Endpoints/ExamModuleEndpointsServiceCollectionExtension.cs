using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Exam.Application;
using Modules.Exam.Infrastructure;
using Serilog;

namespace Modules.Exam.Endpoints;

public static class ExamModuleEndpointsServiceCollectionExtension
{
    public static IServiceCollection RegisterExamEndpointsModule(this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger,
        List<Assembly> mediatRAssembly)
    {
        mediatRAssembly.Add(typeof(ExamModuleEndpointsServiceCollectionExtension).Assembly);
        services.RegisterExamModuleApplication(mediatRAssembly);
        services.RegisterExamModule(configuration, logger, mediatRAssembly);
        return services;
    }
}
