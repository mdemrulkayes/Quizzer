using Modules.Quiz.Application.Question.Question.Dtos;
using Modules.Quiz.Application.Question.QuestionSet.Dtos;
using Modules.Quiz.Application.Tag.Dtos;
using Shared.Application;
using Shared.Core;
using QuestionEntity = Modules.Quiz.Core.QuestionAggregate.Question;

namespace Modules.Quiz.Application.Common.Extensions;

internal static class MappingExtensions
{
    public static TagResponse ToResponse(this Core.Tag.Tag tag) =>
        new(tag.TagId, tag.Name, tag.Description);

    public static QuestionOptionResponse ToResponse(this Core.QuestionAggregate.QuestionOption option) =>
        new(option.QuestionOptionId, option.OptionText, option.IsAnswer, option.OptionIdentifier);

    public static QuestionResponse ToResponse(this QuestionEntity question) =>
        new(question.QuestionId,
            question.AskedQuestion,
            question.Discussion,
            question.QuestionMark,
            question.QuestionType,
            question.Explanation,
            question.DifficultyScore,
            question.Sequence,
            question.Options.Select(o => o.ToResponse()).ToList());

    public static QuestionSetResponse ToResponse(this Core.QuestionAggregate.QuestionSet set) =>
        new(set.QuestionSetId,
            set.Name,
            set.SetCode,
            set.Details,
            set.Source,
            set.IsPublic,
            set.Complexity,
            set.ExperienceYears,
            set.ExpertiseFields,
            set.Questions.Select(q => q.ToResponse()).ToList());

    public static PagedListDto<TDto> ToPagedListDto<TEntity, TDto>(
        this PaginatedList<TEntity> source,
        Func<TEntity, TDto> mapper) =>
        new()
        {
            TotalCount = source.TotalCount,
            Items = source.Items.Select(mapper).ToList().AsReadOnly()
        };
}
