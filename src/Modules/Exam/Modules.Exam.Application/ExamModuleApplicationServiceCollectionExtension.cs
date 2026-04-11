using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Modules.Exam.Application.Services;

namespace Modules.Exam.Application;

public static class ExamModuleApplicationServiceCollectionExtension
{
    public static IServiceCollection RegisterExamModuleApplication(this IServiceCollection services, List<Assembly> mediatRAssembly)
    {
        mediatRAssembly.Add(typeof(ExamModuleApplicationServiceCollectionExtension).Assembly);
        services.AddScoped<IExamGradingService, ExamGradingService>();
        return services;
    }
}
