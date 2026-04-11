using System.Net;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Identity.Constants;
using Modules.Quiz.Application.Question.QuestionSetTag;
using Modules.Quiz.Application.Tag.Dtos;
using Modules.Quiz.Core;
using Shared.Core;

namespace Modules.Quiz.Endpoints.QuestionSetTag;

internal class QuestionSetTagEndpoints : IBaseEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapGet(QuestionModuleConstants.Route.QuestionSetRoute.GetQuestionSetTags, GetQuestionSetTags)
            .Produces((int)HttpStatusCode.OK, typeof(List<TagResponse>))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.QuestionSetTag)
            .RequireAuthorization(AuthorizationPolicyConstants.AuthenticatedPolicy);

        routeBuilder
            .MapPost(QuestionModuleConstants.Route.QuestionSetRoute.AssignTagToQuestionSet, AssignTagToQuestionSet)
            .Produces((int)HttpStatusCode.OK, typeof(TagResponse))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.QuestionSetTag)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);

        routeBuilder
            .MapDelete(QuestionModuleConstants.Route.QuestionSetRoute.RemoveTagFromQuestionSet, RemoveTagFromQuestionSet)
            .Produces((int)HttpStatusCode.OK, typeof(bool))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.QuestionSetTag)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);
    }

    private static async Task<IResult> GetQuestionSetTags(ISender sender, long setId)
    {
        var result = await sender.Send(new GetQuestionSetTagsQuery(setId));
        return result.ConvertToResult();
    }

    private static async Task<IResult> AssignTagToQuestionSet(ISender sender, long setId, AssignTagRequest request)
    {
        var result = await sender.Send(new AssignTagToQuestionSetCommand(setId, request.TagId));
        return result.ConvertToResult();
    }

    private static async Task<IResult> RemoveTagFromQuestionSet(ISender sender, long setId, long tagId)
    {
        var result = await sender.Send(new RemoveTagFromQuestionSetCommand(setId, tagId));
        return result.ConvertToResult();
    }
}

public sealed record AssignTagRequest(long TagId);
