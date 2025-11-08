using AutoMapper;
using Modules.Quiz.Application.Question.Question.Dtos;
using Modules.Quiz.Application.Question.QuestionSet.Dtos;

namespace Modules.Quiz.Application.Question.QuestionSet;

internal sealed class QuestionSetMappingProfile : Profile
{
    public QuestionSetMappingProfile()
    {
        CreateMap<Core.QuestionAggregate.QuestionSet, QuestionSetResponse>()
            .ConstructUsing(set => new QuestionSetResponse(
                set.QuestionSetId,
                set.Name,
                set.SetCode,
                set.Details,
                set.Questions.Select(q => new QuestionResponse(
                    q.QuestionId,
                    q.AskedQuestion,
                    q.Discussion,
                    q.QuestionMark,
                    q.Options.Select(o => new QuestionOptionResponse(
                        o.QuestionOptionId,
                        o.OptionText,
                        o.IsAnswer
                    )).ToList()
                )).ToList()
            ))
            .ReverseMap();
    }
}
