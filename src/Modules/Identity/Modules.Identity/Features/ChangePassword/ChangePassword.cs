using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Identity.Constants;
using Shared.Core;

namespace Modules.Identity.Features.ChangePassword;
internal sealed class ChangePasswordEndpoint : IBaseEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapPut(IdentityModuleConstants.Route.ChangePassword, ChangePasswordHandler)
            .WithName("ChangePassword")
            .WithTags(IdentityModuleConstants.RouteTag.IdentityTagName)
            .RequireAuthorization();
    }

    private static async Task<IResult> ChangePasswordHandler(ChangePasswordCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ConvertToProblemDetails();
    }
}
