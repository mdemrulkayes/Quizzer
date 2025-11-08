using System.Net;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Quiz.Application.Question.Question.Create;
using Modules.Quiz.Application.Question.Question.Delete;
using Modules.Quiz.Application.Question.Question.Dtos;
using Modules.Quiz.Application.Question.Question.Query;
using Modules.Quiz.Application.Question.Question.Update;
using Modules.Quiz.Core;
using Shared.Application;
using Shared.Core;

namespace Modules.Quiz.Endpoints.Question;
internal class QuestionEndpoints : IBaseEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapGet(QuestionModuleConstants.Route.QuestionRoute.GetAllQuestions, GetAllQuestions)
            .Produces((int)HttpStatusCode.OK, typeof(PagedListDto<QuestionResponse>))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.QuestionTag)
            .RequireAuthorization();

        routeBuilder.MapGet(QuestionModuleConstants.Route.QuestionRoute.GetQuestionDetailsById, GetQuestionDetailsById)
            .Produces((int)HttpStatusCode.OK, typeof(QuestionResponse))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.QuestionTag)
            .RequireAuthorization();

        routeBuilder.MapPost(QuestionModuleConstants.Route.QuestionRoute.CreateQuestion, CreateQuestion)
            .Produces((int)HttpStatusCode.OK, typeof(QuestionResponse))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.QuestionTag)
            .RequireAuthorization();

        routeBuilder.MapPut(QuestionModuleConstants.Route.QuestionRoute.UpdateQuestion, UpdateQuestion)
            .Produces((int)HttpStatusCode.OK, typeof(QuestionResponse))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.QuestionTag)
            .RequireAuthorization();

        routeBuilder.MapDelete(QuestionModuleConstants.Route.QuestionRoute.DeleteQuestion, DeleteQuestion)
            .Produces((int)HttpStatusCode.OK, typeof(bool))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.QuestionTag)
            .RequireAuthorization();
    }

    private async Task<IResult> GetAllQuestions(ISender sender, [AsParameters] GetAllQuestionQuery query)
    {
        var questions = await sender.Send(query);
        return questions.ConvertToResult();
    }

    private static async Task<IResult> GetQuestionDetailsById(ISender sender, long questionId)
    {
        var question = await sender.Send(new GetQuestionByIdQuery(questionId));
        return question.ConvertToResult();
    }

    private static async Task<IResult> CreateQuestion(ISender sender, CreateQuestionCommand command)
    {
        var question = await sender.Send(command);

        return question.ConvertToResult();
    }

    private static async Task<IResult> UpdateQuestion(ISender sender, long questionId, UpdateQuestionCommand command)
    {
        if (questionId != command.QuestionId)
        {
            return Results.BadRequest("Invalid request");
        }
        var set = await sender.Send(command);
        return set.ConvertToResult();
    }

    private static async Task<IResult> DeleteQuestion(ISender sender, long questionId)
    {
        var deleteSet = await sender.Send(new DeleteQuestionCommand(questionId));

        return deleteSet.ConvertToResult();
    }
}
