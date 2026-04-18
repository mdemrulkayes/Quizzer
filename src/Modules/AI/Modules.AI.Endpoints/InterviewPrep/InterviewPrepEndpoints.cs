using System.Net;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.AI.Application.Dtos;
using Modules.AI.Application.InterviewPrep.Queries.GetInterviewPrepMaterialById;
using Modules.AI.Application.InterviewPrep.Queries.GetInterviewPrepMaterials;
using Modules.AI.Core;
using Modules.Identity.Constants;
using Shared.Core;

namespace Modules.AI.Endpoints.InterviewPrep;

internal class InterviewPrepEndpoints : IBaseEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet(AIModuleConstants.Route.InterviewPrep.GetAll, GetInterviewPrepMaterials)
            .Produces((int)HttpStatusCode.OK, typeof(List<InterviewPrepMaterialDto>))
            .WithTags(AIModuleConstants.RouteTag.InterviewPrepTag)
            .RequireAuthorization(AuthorizationPolicyConstants.AuthenticatedPolicy);

        routeBuilder.MapGet(AIModuleConstants.Route.InterviewPrep.GetById, GetInterviewPrepMaterialById)
            .Produces((int)HttpStatusCode.OK, typeof(InterviewPrepMaterialDetailDto))
            .WithTags(AIModuleConstants.RouteTag.InterviewPrepTag)
            .RequireAuthorization(AuthorizationPolicyConstants.AuthenticatedPolicy);
    }

    private static async Task<IResult> GetInterviewPrepMaterials(
        ISender sender,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new GetInterviewPrepMaterialsQuery(pageNumber, pageSize));
        return result.ConvertToResult();
    }

    private static async Task<IResult> GetInterviewPrepMaterialById(ISender sender, Guid id)
    {
        var result = await sender.Send(new GetInterviewPrepMaterialByIdQuery(id));
        return result.ConvertToResult();
    }
}
