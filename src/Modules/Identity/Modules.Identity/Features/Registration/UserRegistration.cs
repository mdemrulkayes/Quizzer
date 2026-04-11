using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Identity.Constants;
using Shared.Core;

namespace Modules.Identity.Features.Registration;

internal class UserRegistration : IBaseEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost(IdentityModuleConstants.Route.Register, RegisterUser)
            .WithName(nameof(IdentityModuleConstants.Route.Register))
            .WithTags(IdentityModuleConstants.RouteTag.IdentityTagName);
    }

    private static async Task<IResult> RegisterUser(UserRegistrationCommand command, IMediator mediator)
    {
        var userRegistrationResult = await mediator.Send(command);
        return userRegistrationResult.IsSuccess
            ? TypedResults.Ok()
            : userRegistrationResult.ConvertToProblemDetails();
    }
}
