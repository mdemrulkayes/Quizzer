using System.Net;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Identity.Constants;
using Modules.Quiz.Application.Question.QuestionSet.Create;
using Modules.Quiz.Application.Question.QuestionSet.Delete;
using Modules.Quiz.Application.Question.QuestionSet.Dtos;
using Modules.Quiz.Application.Question.QuestionSet.Query;
using Modules.Quiz.Application.Question.QuestionSet.Update;
using Modules.Quiz.Core;
using Shared.Application;
using Shared.Core;

namespace Modules.Quiz.Endpoints.Question;
internal class QuestionSetEndpoints : IBaseEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapGet(QuestionModuleConstants.Route.QuestionSetRoute.GetAllQuestionSets, GetAllQuestionSets)
            .Produces((int)HttpStatusCode.OK, typeof(PagedListDto<QuestionSetResponse>))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.QuestionSetTag)
            .RequireAuthorization(AuthorizationPolicyConstants.AuthenticatedPolicy);

        routeBuilder.MapGet(QuestionModuleConstants.Route.QuestionSetRoute.GetQuestionSetDetailsById, GetQuestionSetDetailsById)
            .Produces((int)HttpStatusCode.OK, typeof(QuestionSetResponse))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.QuestionSetTag)
            .RequireAuthorization(AuthorizationPolicyConstants.AuthenticatedPolicy);

        routeBuilder.MapPost(QuestionModuleConstants.Route.QuestionSetRoute.CreateQuestionSet, CreateQuestionSet)
            .Produces((int)HttpStatusCode.OK, typeof(QuestionSetResponse))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.QuestionSetTag)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);

        routeBuilder.MapPut(QuestionModuleConstants.Route.QuestionSetRoute.UpdateQuestionSet, UpdateQuestionSet)
            .Produces((int)HttpStatusCode.OK, typeof(QuestionSetResponse))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.QuestionSetTag)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);

        routeBuilder.MapDelete(QuestionModuleConstants.Route.QuestionSetRoute.DeleteQuestionSet, DeleteQuestionSet)
            .Produces((int)HttpStatusCode.OK, typeof(bool))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.QuestionSetTag)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);
    }

    private async Task<IResult> GetAllQuestionSets(ISender sender, [AsParameters] GetAllQuestionSetQuery query)
    {
        var sets = await sender.Send(query);
        return sets.ConvertToResult();
    }

    private static async Task<IResult> GetQuestionSetDetailsById(ISender sender, long setId)
    {
        var set = await sender.Send(new GetQuestionSetByIdQuery(setId));
        return set.ConvertToResult();
    }

    private static async Task<IResult> CreateQuestionSet(ISender sender, CreateQuestionSetCommand command)
    {
        var set = await sender.Send(command);

        return set.ConvertToResult();
    }

    private static async Task<IResult> UpdateQuestionSet(ISender sender, long setId, UpdateQuestionSetCommand command)
    {
        if (setId != command.QuestionSetId)
        {
            return Results.BadRequest("Invalid request");
        }
        var set = await sender.Send(command);
        return set.ConvertToResult();
    }

    private static async Task<IResult> DeleteQuestionSet(ISender sender, long setId)
    {
        var deleteSet = await sender.Send(new DeleteQuestionSetCommand(setId));

        return deleteSet.ConvertToResult();
    }
}
