using Microsoft.EntityFrameworkCore;
using Modules.Exam.Core.ExamAggregate;

namespace Modules.Exam.Infrastructure.Persistence;

public sealed class ExamModuleDbContext : DbContext
{
    public DbSet<Core.ExamAggregate.Exam> Exams => Set<Core.ExamAggregate.Exam>();
    public DbSet<ExamAttempt> ExamAttempts => Set<ExamAttempt>();
    public DbSet<ExamAnswer> ExamAnswers => Set<ExamAnswer>();

    public ExamModuleDbContext(DbContextOptions<ExamModuleDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(ExamModuleConstants.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExamModuleDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
