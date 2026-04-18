using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.AI.Application;
using Modules.AI.Infrastructure;
using Serilog;
using System.Reflection;

namespace Modules.AI.Endpoints;

public static class AIModuleEndpointsServiceCollectionExtension
{
    public static IServiceCollection RegisterAIModule(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger,
        List<Assembly> mediatRAssemblies)
    {
        mediatRAssemblies.Add(typeof(AIModuleEndpointsServiceCollectionExtension).Assembly);
        services.RegisterAIModuleApplication(mediatRAssemblies);
        services.RegisterAIModuleInfrastructure(configuration, logger, mediatRAssemblies);
        return services;
    }
}
