using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Identity.Constants;
using Shared.Core;

namespace Modules.Identity.Features.UserManagement;
internal sealed class UserManagementEndpoints : IBaseEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapGet(IdentityModuleConstants.Route.GetAllUsers, GetAllUsers)
            .WithName("GetAllUsers")
            .WithTags(IdentityModuleConstants.RouteTag.UserManagementTagName)
            .RequireAuthorization(AuthorizationPolicyConstants.AdminPolicy);

        routeBuilder
            .MapGet(IdentityModuleConstants.Route.GetUserById, GetUserById)
            .WithName("GetUserById")
            .WithTags(IdentityModuleConstants.RouteTag.UserManagementTagName)
            .RequireAuthorization(AuthorizationPolicyConstants.AdminPolicy);

        routeBuilder
            .MapPut(IdentityModuleConstants.Route.UpdateUserRole, UpdateUserRole)
            .WithName("UpdateUserRole")
            .WithTags(IdentityModuleConstants.RouteTag.UserManagementTagName)
            .RequireAuthorization(AuthorizationPolicyConstants.SuperAdminPolicy);

        routeBuilder
            .MapDelete(IdentityModuleConstants.Route.DeleteUser, DeleteUser)
            .WithName("DeleteUser")
            .WithTags(IdentityModuleConstants.RouteTag.UserManagementTagName)
            .RequireAuthorization(AuthorizationPolicyConstants.AdminPolicy);
    }

    private static async Task<IResult> GetAllUsers(ISender sender, int pageNumber = 1, int pageSize = 10)
    {
        var result = await sender.Send(new GetAllUsersQuery(pageNumber, pageSize));
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ConvertToProblemDetails();
    }

    private static async Task<IResult> GetUserById(ISender sender, Guid userId)
    {
        var result = await sender.Send(new GetUserByIdQuery(userId));
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ConvertToProblemDetails();
    }

    private static async Task<IResult> UpdateUserRole(ISender sender, Guid userId, UpdateUserRoleCommand command)
    {
        if (userId != command.UserId)
        {
            return Results.BadRequest("Invalid request");
        }
        var result = await sender.Send(command);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ConvertToProblemDetails();
    }

    private static async Task<IResult> DeleteUser(ISender sender, Guid userId)
    {
        var result = await sender.Send(new DeleteUserCommand(userId));
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ConvertToProblemDetails();
    }
}
