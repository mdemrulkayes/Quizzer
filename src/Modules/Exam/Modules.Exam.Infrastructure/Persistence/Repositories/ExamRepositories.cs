using Modules.Exam.Core.ExamAggregate;

namespace Modules.Exam.Infrastructure.Persistence.Repositories;

internal sealed class ExamRepository(ExamModuleDbContext context) : BaseRepository<Core.ExamAggregate.Exam>(context), IExamRepository;

internal sealed class ExamAttemptRepository(ExamModuleDbContext context) : BaseRepository<ExamAttempt>(context), IExamAttemptRepository;
