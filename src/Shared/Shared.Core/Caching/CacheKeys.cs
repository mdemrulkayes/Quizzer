namespace Shared.Core.Caching;

/// <summary>
/// Centralised cache key prefixes for consistent invalidation.
/// </summary>
public static class CacheKeys
{
    public const string Tags = "tags";
    public const string Questions = "questions";
    public const string QuestionSets = "question-sets";
    public const string Exams = "exams";
    public const string ExamResults = "exam-results";
    public const string Users = "users";
}
