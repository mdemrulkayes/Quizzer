using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Identity.Constants;
using Shared.Core;

namespace Modules.Identity.Features.RefreshToken;
internal sealed class RefreshTokenEndpoint : IBaseEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapPost(IdentityModuleConstants.Route.RefreshToken, RefreshTokenHandler)
            .WithName("RefreshToken")
            .WithTags(IdentityModuleConstants.RouteTag.IdentityTagName)
            .AllowAnonymous();
    }

    private static async Task<IResult> RefreshTokenHandler(RefreshTokenCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ConvertToProblemDetails();
    }
}
