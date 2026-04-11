using System.Net;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Identity.Constants;
using Modules.Quiz.Application.Tag.Create;
using Modules.Quiz.Application.Tag.Delete;
using Modules.Quiz.Application.Tag.Dtos;
using Modules.Quiz.Application.Tag.Query;
using Modules.Quiz.Application.Tag.Update;
using Modules.Quiz.Core;
using Shared.Application;
using Shared.Core;

namespace Modules.Quiz.Endpoints.Tag;
internal class Tag : IBaseEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapGet(QuestionModuleConstants.Route.TagRoute.GetAllTags, GetAllTags)
            .Produces((int)HttpStatusCode.OK, typeof(PagedListDto<TagResponse>))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.TagEndPointTagName)
            .RequireAuthorization(AuthorizationPolicyConstants.AuthenticatedPolicy);

        routeBuilder.MapGet(QuestionModuleConstants.Route.TagRoute.GetTagDetailsById, GetTagDetailsById)
            .Produces((int)HttpStatusCode.OK, typeof(TagResponse))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.TagEndPointTagName)
            .RequireAuthorization(AuthorizationPolicyConstants.AuthenticatedPolicy);

        routeBuilder.MapPost(QuestionModuleConstants.Route.TagRoute.CreateTag, CreateTag)
            .Produces((int)HttpStatusCode.OK, typeof(TagResponse))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.TagEndPointTagName)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);

        routeBuilder.MapPut(QuestionModuleConstants.Route.TagRoute.UpdateTag, UpdateTag)
            .Produces((int)HttpStatusCode.OK, typeof(TagResponse))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.TagEndPointTagName)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);

        routeBuilder.MapDelete(QuestionModuleConstants.Route.TagRoute.DeleteTag, DeleteTag)
            .Produces((int)HttpStatusCode.OK, typeof(bool))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.TagEndPointTagName)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);
    }

    private async Task<IResult> GetAllTags(ISender sender, [AsParameters] GetAllTagQuery query)
    {
        var allTags = await sender.Send(query);
        return allTags.ConvertToResult();
    }

    private static async Task<IResult> GetTagDetailsById(ISender sender, long tagId)
    {
        var tag = await sender.Send(new GetTagByIdQuery(tagId));
        return tag.ConvertToResult();
    }

    private static async Task<IResult> CreateTag(ISender sender, CreateTagCommand command)
    {
        var createdTag = await sender.Send(command);

        return createdTag.ConvertToResult();
    }

    private static async Task<IResult> UpdateTag(ISender sender, long tagId, UpdateTagCommand command)
    {
        if (tagId != command.TagId)
        {
            return Results.BadRequest("Invalid request");
        }
        var updatedTag = await sender.Send(command);
        return updatedTag.ConvertToResult();
    }

    private static async Task<IResult> DeleteTag(ISender sender, long tagId)
    {
        var deleteTag = await sender.Send(new DeleteTagCommand(tagId));

        return deleteTag.ConvertToResult();
    }
}
