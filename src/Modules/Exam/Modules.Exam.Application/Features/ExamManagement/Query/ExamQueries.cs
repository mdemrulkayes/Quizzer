using Modules.Exam.Application.Features.ExamManagement.Dtos;
using Shared.Core;

namespace Modules.Exam.Application.Features.ExamManagement.Query;

public sealed record GetAllExamsQuery(int PageNumber = 1, int PageSize = 10) : IQuery<Result<PaginatedList<ExamResponse>>>;

public sealed record GetExamByIdQuery(long ExamId) : IQuery<Result<ExamResponse>>;
