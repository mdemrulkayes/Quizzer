namespace Shared.Core.ModuleServices;

public interface IQuestionQueryService
{
    Task<bool> QuestionSetExistsAsync(long questionSetId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuestionDto>> GetQuestionsBySetIdAsync(long questionSetId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuestionOptionDto>> GetOptionsByQuestionIdsAsync(IEnumerable<long> questionIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CorrectAnswerDto>> GetCorrectAnswersBySetIdAsync(long questionSetId, CancellationToken cancellationToken = default);
}

public sealed record QuestionDto(long QuestionId, string AskedQuestion, int? QuestionMark);

public sealed record QuestionOptionDto(long QuestionOptionId, string OptionText, long QuestionId);

public sealed record CorrectAnswerDto(long QuestionOptionId, long QuestionId);
