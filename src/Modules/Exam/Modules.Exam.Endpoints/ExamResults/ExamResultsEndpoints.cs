using System.Net;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Exam.Application.Features.ExamManagement.Dtos;
using Modules.Exam.Application.Features.ExamResults.Dtos;
using Modules.Exam.Application.Features.ExamResults.Query;
using Modules.Exam.Core.ExamAggregate;
using Modules.Identity.Constants;
using Shared.Core;

namespace Modules.Exam.Endpoints.ExamResults;

internal sealed class ExamResultsEndpoints : IBaseEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapGet(ExamModuleConstants.Route.GetMyResult, GetMyResult)
            .Produces((int)HttpStatusCode.OK, typeof(ExamResultResponse))
            .ProducesValidationProblem()
            .WithTags(ExamModuleConstants.RouteTag.ExamResults)
            .RequireAuthorization(AuthorizationPolicyConstants.ExaminePolicy);

        routeBuilder
            .MapGet(ExamModuleConstants.Route.GetExamResults, GetExamResults)
            .Produces((int)HttpStatusCode.OK, typeof(PaginatedList<ExamAttemptResponse>))
            .WithTags(ExamModuleConstants.RouteTag.ExamResults)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);

        routeBuilder
            .MapGet(ExamModuleConstants.Route.GetMyAllResults, GetMyAllResults)
            .Produces((int)HttpStatusCode.OK, typeof(PaginatedList<ExamAttemptResponse>))
            .WithTags(ExamModuleConstants.RouteTag.ExamResults)
            .RequireAuthorization(AuthorizationPolicyConstants.ExaminePolicy);
    }

    private static async Task<IResult> GetMyResult(ISender sender, long examId)
    {
        var result = await sender.Send(new GetMyExamResultQuery(examId));
        return result.ConvertToResult();
    }

    private static async Task<IResult> GetExamResults(ISender sender, long examId, int pageNumber = 1, int pageSize = 10)
    {
        var result = await sender.Send(new GetExamResultsQuery(examId, pageNumber, pageSize));
        return result.ConvertToResult();
    }

    private static async Task<IResult> GetMyAllResults(ISender sender, int pageNumber = 1, int pageSize = 10)
    {
        var result = await sender.Send(new GetMyAllResultsQuery(pageNumber, pageSize));
        return result.ConvertToResult();
    }
}
