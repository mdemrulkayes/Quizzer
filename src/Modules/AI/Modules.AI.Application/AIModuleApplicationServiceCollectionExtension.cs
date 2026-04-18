using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.AI.Application;

public static class AIModuleApplicationServiceCollectionExtension
{
    public static IServiceCollection RegisterAIModuleApplication(
        this IServiceCollection services,
        List<Assembly> mediatRAssemblies)
    {
        mediatRAssemblies.Add(typeof(AIModuleApplicationServiceCollectionExtension).Assembly);
        services.AddValidatorsFromAssembly(typeof(AIModuleApplicationServiceCollectionExtension).Assembly);
        return services;
    }
}
