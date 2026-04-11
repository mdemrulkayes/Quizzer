using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Exam.Infrastructure.Persistence;
using Shared.Core.IntegrationEvents;
using Shared.Core.IntegrationEvents.Events;

namespace Modules.Exam.Application.Features.IntegrationEventHandlers;

internal sealed class QuestionSetDeletedHandler(
    ExamModuleDbContext dbContext,
    ILogger<QuestionSetDeletedHandler> logger) : IIntegrationEventHandler<QuestionSetDeletedEvent>
{
    public async Task HandleAsync(QuestionSetDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        var affectedExams = await dbContext.Exams
            .Where(e => e.QuestionSetId == @event.QuestionSetId && e.IsPublished)
            .ToListAsync(cancellationToken);

        if (affectedExams.Count == 0)
        {
            logger.LogInformation(
                "No published exams found for deleted QuestionSet {QuestionSetId}",
                @event.QuestionSetId);
            return;
        }

        foreach (var exam in affectedExams)
        {
            exam.Unpublish();
            logger.LogWarning(
                "Unpublished exam {ExamId} ('{Title}') because QuestionSet {QuestionSetId} was deleted",
                exam.ExamId, exam.Title, @event.QuestionSetId);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Unpublished {Count} exam(s) due to deletion of QuestionSet {QuestionSetId}",
            affectedExams.Count, @event.QuestionSetId);
    }
}
