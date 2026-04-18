using System.Net;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.AI.Application.Dtos;
using Modules.AI.Application.ProviderConfig.Commands.DeleteProviderConfig;
using Modules.AI.Application.ProviderConfig.Commands.SaveProviderConfig;
using Modules.AI.Application.ProviderConfig.Commands.TestProviderConnection;
using Modules.AI.Application.ProviderConfig.Queries.GetProviderConfig;
using Modules.AI.Application.ProviderConfig.Queries.GetSupportedProviders;
using Modules.AI.Core;
using Modules.Identity.Constants;
using Shared.Core;

namespace Modules.AI.Endpoints.ProviderConfig;

internal class ProviderConfigEndpoints : IBaseEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet(AIModuleConstants.Route.ProviderConfig.GetSupportedProviders, GetSupportedProviders)
            .Produces((int)HttpStatusCode.OK, typeof(List<SupportedProviderDto>))
            .WithTags(AIModuleConstants.RouteTag.ProviderConfigTag)
            .RequireAuthorization(AuthorizationPolicyConstants.AuthenticatedPolicy);

        routeBuilder.MapGet(AIModuleConstants.Route.ProviderConfig.GetProviderConfig, GetProviderConfig)
            .Produces((int)HttpStatusCode.OK, typeof(ProviderConfigResponse))
            .WithTags(AIModuleConstants.RouteTag.ProviderConfigTag)
            .RequireAuthorization(AuthorizationPolicyConstants.AuthenticatedPolicy);

        routeBuilder.MapPost(AIModuleConstants.Route.ProviderConfig.SaveProviderConfig, SaveProviderConfig)
            .Produces((int)HttpStatusCode.OK, typeof(ProviderConfigResponse))
            .ProducesValidationProblem()
            .WithTags(AIModuleConstants.RouteTag.ProviderConfigTag)
            .RequireAuthorization(AuthorizationPolicyConstants.AuthenticatedPolicy);

        routeBuilder.MapDelete(AIModuleConstants.Route.ProviderConfig.DeleteProviderConfig, DeleteProviderConfig)
            .Produces((int)HttpStatusCode.OK, typeof(bool))
            .WithTags(AIModuleConstants.RouteTag.ProviderConfigTag)
            .RequireAuthorization(AuthorizationPolicyConstants.AuthenticatedPolicy);

        routeBuilder.MapPost(AIModuleConstants.Route.ProviderConfig.TestProviderConnection, TestProviderConnection)
            .Produces((int)HttpStatusCode.OK, typeof(TestConnectionResponse))
            .WithTags(AIModuleConstants.RouteTag.ProviderConfigTag)
            .RequireAuthorization(AuthorizationPolicyConstants.AuthenticatedPolicy);
    }

    private static async Task<IResult> GetSupportedProviders(ISender sender)
    {
        var result = await sender.Send(new GetSupportedProvidersQuery());
        return result.ConvertToResult();
    }

    private static async Task<IResult> GetProviderConfig(ISender sender)
    {
        var result = await sender.Send(new GetProviderConfigQuery());
        return result.ConvertToResult();
    }

    private static async Task<IResult> SaveProviderConfig(ISender sender, SaveProviderConfigCommand command)
    {
        var result = await sender.Send(command);
        return result.ConvertToResult();
    }

    private static async Task<IResult> DeleteProviderConfig(ISender sender)
    {
        var result = await sender.Send(new DeleteProviderConfigCommand());
        return result.ConvertToResult();
    }

    private static async Task<IResult> TestProviderConnection(ISender sender)
    {
        var result = await sender.Send(new TestProviderConnectionCommand());
        return result.ConvertToResult();
    }
}
