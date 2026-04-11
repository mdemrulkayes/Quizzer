using Microsoft.EntityFrameworkCore;
using Modules.Exam.Application.Features.ExamManagement.Dtos;
using Modules.Exam.Core.ExamAggregate;
using Modules.Exam.Infrastructure.Persistence;
using Shared.Core;
using Shared.Core.Extensions;

namespace Modules.Exam.Application.Features.ExamManagement.Query;

internal sealed class GetAllExamsQueryHandler(ExamModuleDbContext dbContext)
    : IQueryHandler<GetAllExamsQuery, Result<PaginatedList<ExamResponse>>>
{
    public async Task<Result<PaginatedList<ExamResponse>>> Handle(GetAllExamsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Exams
            .OrderByDescending(e => e.CreatedDate)
            .Select(e => new ExamResponse(
                e.ExamId, e.Title, e.Description, e.QuestionSetId,
                e.DurationInMinutes, e.TotalMarks, e.PassingMarks,
                e.IsPublished, e.ScheduledStartTime, e.ScheduledEndTime));

        return await query.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}

internal sealed class GetExamByIdQueryHandler(ExamModuleDbContext dbContext)
    : IQueryHandler<GetExamByIdQuery, Result<ExamResponse>>
{
    public async Task<Result<ExamResponse>> Handle(GetExamByIdQuery request, CancellationToken cancellationToken)
    {
        var exam = await dbContext.Exams
            .Where(e => e.ExamId == request.ExamId)
            .Select(e => new ExamResponse(
                e.ExamId, e.Title, e.Description, e.QuestionSetId,
                e.DurationInMinutes, e.TotalMarks, e.PassingMarks,
                e.IsPublished, e.ScheduledStartTime, e.ScheduledEndTime))
            .FirstOrDefaultAsync(cancellationToken);

        return exam is null ? ExamErrors.ExamNotFound : exam;
    }
}
