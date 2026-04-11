namespace Shared.Core;
public interface IUnitOfWork : IDisposable
{
    Task<int> CommitAsync(CancellationToken cancellationToken);
}

public static class ModuleKeys
{
    public const string Quiz = "Quiz";
    public const string Exam = "Exam";
}
