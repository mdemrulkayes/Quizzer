using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Modules.Identity.Constants;
using Shared.Core;

namespace Modules.Identity.Features.Login;
internal sealed class Login : IBaseEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapPost(IdentityModuleConstants.Route.Login, LoginHandler)
            // .Produces<AccessTokenResponse>()
            .WithName(nameof(IdentityModuleConstants.Route.Login))
            .WithTags(IdentityModuleConstants.RouteTag.IdentityTagName);
    }

    private static async Task<IResult> LoginHandler(LoginCommand command, IMediator mediator, ILogger<Login> logger)
    {
        logger.LogInformation("Login request received");
        var result = await mediator.Send(command);
        logger.LogInformation("Login request completed, success: {ResponseIsSuccess}", result.IsSuccess);
        return result.IsSuccess ?
            TypedResults.Ok(result.Value) :
            result.ConvertToProblemDetails();
    }
}
