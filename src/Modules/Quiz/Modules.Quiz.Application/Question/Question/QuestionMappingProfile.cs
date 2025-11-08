using AutoMapper;
using Modules.Quiz.Application.Question.Question.Dtos;

namespace Modules.Quiz.Application.Question.Question;
internal sealed class QuestionMappingProfile : Profile
{
    public QuestionMappingProfile()
    {
        CreateMap<Core.QuestionAggregate.Question, QuestionResponse>()
            .ConstructUsing(set => new QuestionResponse(
                set.QuestionId,
                set.AskedQuestion,
                set.Discussion,
                set.QuestionMark,
                set.Options.Select(option => new QuestionOptionResponse(
                    option.QuestionOptionId,
                    option.OptionText,
                    option.IsAnswer
                )).ToList()
            ))
            .ReverseMap();
    }
}
