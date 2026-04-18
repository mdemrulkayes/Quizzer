using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Modules.Exam.Application.Features.IntegrationEventHandlers;
using Modules.Exam.Application.Services;
using Shared.Core.IntegrationEvents;
using Shared.Core.IntegrationEvents.Events;

namespace Modules.Exam.Application;

public static class ExamModuleApplicationServiceCollectionExtension
{
    public static IServiceCollection RegisterExamModuleApplication(this IServiceCollection services, List<Assembly> mediatRAssembly)
    {
        mediatRAssembly.Add(typeof(ExamModuleApplicationServiceCollectionExtension).Assembly);
        services.AddScoped<IExamGradingService, ExamGradingService>();

        // Integration event handlers
        services.AddScoped<IIntegrationEventHandler<QuestionSetDeletedEvent>, QuestionSetDeletedHandler>();
        services.AddScoped<IIntegrationEventHandler<UserDeletedEvent>, UserDeletedHandler>();

        return services;
    }
}
