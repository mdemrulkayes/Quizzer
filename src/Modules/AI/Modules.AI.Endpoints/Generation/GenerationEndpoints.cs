using System.Net;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.AI.Application.Dtos;
using Modules.AI.Application.Generation.Commands.GenerateFromJobDescription;
using Modules.AI.Application.Generation.Commands.GenerateQuestionSet;
using Modules.AI.Application.Generation.Queries.GetGenerationHistory;
using Modules.AI.Core;
using Modules.Identity.Constants;
using Shared.Core;

namespace Modules.AI.Endpoints.Generation;

internal class GenerationEndpoints : IBaseEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost(AIModuleConstants.Route.Generation.GenerateQuestionSet, GenerateQuestionSet)
            .Produces((int)HttpStatusCode.OK, typeof(GenerateQuestionSetResponse))
            .ProducesValidationProblem()
            .WithTags(AIModuleConstants.RouteTag.GenerationTag)
            .RequireAuthorization(AuthorizationPolicyConstants.AuthenticatedPolicy);

        routeBuilder.MapPost(AIModuleConstants.Route.Generation.GenerateFromJobDescription, GenerateFromJobDescription)
            .Produces((int)HttpStatusCode.OK, typeof(GenerateFromJobDescriptionResponse))
            .ProducesValidationProblem()
            .WithTags(AIModuleConstants.RouteTag.GenerationTag)
            .RequireAuthorization(AuthorizationPolicyConstants.AuthenticatedPolicy);

        routeBuilder.MapGet(AIModuleConstants.Route.Generation.GetGenerationHistory, GetGenerationHistory)
            .Produces((int)HttpStatusCode.OK, typeof(List<GenerationHistoryItemDto>))
            .WithTags(AIModuleConstants.RouteTag.GenerationTag)
            .RequireAuthorization(AuthorizationPolicyConstants.AuthenticatedPolicy);
    }

    private static async Task<IResult> GenerateQuestionSet(ISender sender, GenerateQuestionSetCommand command)
    {
        var result = await sender.Send(command);
        return result.ConvertToResult();
    }

    private static async Task<IResult> GenerateFromJobDescription(ISender sender, GenerateFromJobDescriptionCommand command)
    {
        var result = await sender.Send(command);
        return result.ConvertToResult();
    }

    private static async Task<IResult> GetGenerationHistory(
        ISender sender,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new GetGenerationHistoryQuery(pageNumber, pageSize));
        return result.ConvertToResult();
    }
}
