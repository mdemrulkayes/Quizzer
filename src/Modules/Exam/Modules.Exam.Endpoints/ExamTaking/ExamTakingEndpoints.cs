using System.Net;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Exam.Application.Features.ExamManagement.Dtos;
using Modules.Exam.Application.Features.ExamManagement.Query;
using Modules.Exam.Application.Features.ExamTaking.Answer;
using Modules.Exam.Application.Features.ExamTaking.Start;
using Modules.Exam.Application.Features.ExamTaking.Submit;
using Modules.Exam.Core.ExamAggregate;
using Modules.Identity.Constants;
using Shared.Core;

namespace Modules.Exam.Endpoints.ExamTaking;

internal sealed class ExamTakingEndpoints : IBaseEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapGet(ExamModuleConstants.Route.GetAvailableExams, GetAvailableExams)
            .Produces((int)HttpStatusCode.OK, typeof(PaginatedList<ExamResponse>))
            .WithTags(ExamModuleConstants.RouteTag.ExamTaking)
            .RequireAuthorization(AuthorizationPolicyConstants.ExaminePolicy);

        routeBuilder
            .MapPost(ExamModuleConstants.Route.StartExam, StartExam)
            .Produces((int)HttpStatusCode.OK, typeof(ExamAttemptStartResponse))
            .ProducesValidationProblem()
            .WithTags(ExamModuleConstants.RouteTag.ExamTaking)
            .RequireAuthorization(AuthorizationPolicyConstants.ExaminePolicy);

        routeBuilder
            .MapPost(ExamModuleConstants.Route.SubmitAnswer, SubmitAnswer)
            .Produces((int)HttpStatusCode.OK, typeof(bool))
            .ProducesValidationProblem()
            .WithTags(ExamModuleConstants.RouteTag.ExamTaking)
            .RequireAuthorization(AuthorizationPolicyConstants.ExaminePolicy);

        routeBuilder
            .MapPost(ExamModuleConstants.Route.SubmitExam, SubmitExam)
            .Produces((int)HttpStatusCode.OK, typeof(ExamSubmitResponse))
            .ProducesValidationProblem()
            .WithTags(ExamModuleConstants.RouteTag.ExamTaking)
            .RequireAuthorization(AuthorizationPolicyConstants.ExaminePolicy);
    }

    private static async Task<IResult> GetAvailableExams(ISender sender, int pageNumber = 1, int pageSize = 10)
    {
        var result = await sender.Send(new GetAllExamsQuery(pageNumber, pageSize));
        return result.ConvertToResult();
    }

    private static async Task<IResult> StartExam(ISender sender, long examId)
    {
        var result = await sender.Send(new StartExamCommand(examId));
        return result.ConvertToResult();
    }

    private static async Task<IResult> SubmitAnswer(ISender sender, long examId, SubmitAnswerCommand command)
    {
        if (examId != command.ExamId)
            return Results.BadRequest("Invalid request");

        var result = await sender.Send(command);
        return result.ConvertToResult();
    }

    private static async Task<IResult> SubmitExam(ISender sender, long examId)
    {
        var result = await sender.Send(new SubmitExamCommand(examId));
        return result.ConvertToResult();
    }
}
