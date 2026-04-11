using Shared.Core;

namespace Modules.Exam.Core.ExamAggregate;

public interface IExamRepository : IRepository<Exam>;
public interface IExamAttemptRepository : IRepository<ExamAttempt>;
