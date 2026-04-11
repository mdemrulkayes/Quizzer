using System.Net;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Identity.Constants;
using Modules.Quiz.Application.Question.Question.Dtos;
using Modules.Quiz.Application.Question.QuestionOption;
using Modules.Quiz.Core;
using Shared.Core;

namespace Modules.Quiz.Endpoints.QuestionOption;

internal class QuestionOptionEndpoints : IBaseEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapPost(QuestionModuleConstants.Route.QuestionRoute.AddOption, AddOption)
            .Produces((int)HttpStatusCode.OK, typeof(QuestionOptionResponse))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.QuestionTag)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);

        routeBuilder
            .MapPut(QuestionModuleConstants.Route.QuestionRoute.UpdateOption, UpdateOption)
            .Produces((int)HttpStatusCode.OK, typeof(QuestionOptionResponse))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.QuestionTag)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);

        routeBuilder
            .MapDelete(QuestionModuleConstants.Route.QuestionRoute.DeleteOption, DeleteOption)
            .Produces((int)HttpStatusCode.OK, typeof(bool))
            .ProducesValidationProblem()
            .WithTags(QuestionModuleConstants.RouteTag.QuestionTag)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);
    }

    private static async Task<IResult> AddOption(ISender sender, long questionId, AddOptionRequest request)
    {
        var result = await sender.Send(new AddOptionCommand(questionId, request.OptionText, request.IsAnswer));
        return result.ConvertToResult();
    }

    private static async Task<IResult> UpdateOption(ISender sender, long questionId, long optionId, UpdateOptionRequest request)
    {
        var result = await sender.Send(new UpdateOptionCommand(questionId, optionId, request.OptionText, request.IsAnswer));
        return result.ConvertToResult();
    }

    private static async Task<IResult> DeleteOption(ISender sender, long questionId, long optionId)
    {
        var result = await sender.Send(new DeleteOptionCommand(questionId, optionId));
        return result.ConvertToResult();
    }
}

public sealed record AddOptionRequest(string OptionText, bool IsAnswer);
public sealed record UpdateOptionRequest(string OptionText, bool IsAnswer);
