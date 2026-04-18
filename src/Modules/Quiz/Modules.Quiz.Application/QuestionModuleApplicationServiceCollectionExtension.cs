using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Modules.Quiz.Application.Question.QuestionSet.IntegrationEventHandlers;
using Shared.Core.IntegrationEvents;
using Shared.Core.IntegrationEvents.Events;

namespace Modules.Quiz.Application;
public static class QuestionModuleApplicationServiceCollectionExtension
{
    public static IServiceCollection RegisterQuestionModuleApplication(this IServiceCollection services, List<Assembly> mediatRAssembly)
    {
        mediatRAssembly.Add(typeof(QuestionModuleApplicationServiceCollectionExtension).Assembly);

        // Integration event handlers
        services.AddScoped<IIntegrationEventHandler<AIQuestionSetGeneratedEvent>, AIQuestionSetGeneratedHandler>();

        return services;
    }
}
