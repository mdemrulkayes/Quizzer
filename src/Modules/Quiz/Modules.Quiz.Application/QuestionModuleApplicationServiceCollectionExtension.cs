using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Quiz.Application;
public static class QuestionModuleApplicationServiceCollectionExtension
{
    public static IServiceCollection RegisterQuestionModuleApplication(this IServiceCollection services, List<Assembly> mediatRAssembly)
    {
        mediatRAssembly.Add(typeof(QuestionModuleApplicationServiceCollectionExtension).Assembly);
        return services;
    }
}
