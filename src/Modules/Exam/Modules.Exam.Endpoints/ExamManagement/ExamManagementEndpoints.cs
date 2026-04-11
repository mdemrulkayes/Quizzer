using System.Net;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Exam.Application.Features.ExamManagement.Create;
using Modules.Exam.Application.Features.ExamManagement.Delete;
using Modules.Exam.Application.Features.ExamManagement.Dtos;
using Modules.Exam.Application.Features.ExamManagement.Publish;
using Modules.Exam.Application.Features.ExamManagement.Query;
using Modules.Exam.Application.Features.ExamManagement.Update;
using Modules.Exam.Core.ExamAggregate;
using Modules.Identity.Constants;
using Shared.Core;

namespace Modules.Exam.Endpoints.ExamManagement;

internal sealed class ExamManagementEndpoints : IBaseEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapGet(ExamModuleConstants.Route.GetAllExams, GetAllExams)
            .Produces((int)HttpStatusCode.OK, typeof(PaginatedList<ExamResponse>))
            .ProducesValidationProblem()
            .WithTags(ExamModuleConstants.RouteTag.ExamManagement)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);

        routeBuilder
            .MapGet(ExamModuleConstants.Route.GetExamById, GetExamById)
            .Produces((int)HttpStatusCode.OK, typeof(ExamResponse))
            .ProducesValidationProblem()
            .WithTags(ExamModuleConstants.RouteTag.ExamManagement)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);

        routeBuilder
            .MapPost(ExamModuleConstants.Route.CreateExam, CreateExam)
            .Produces((int)HttpStatusCode.OK, typeof(ExamResponse))
            .ProducesValidationProblem()
            .WithTags(ExamModuleConstants.RouteTag.ExamManagement)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);

        routeBuilder
            .MapPut(ExamModuleConstants.Route.UpdateExam, UpdateExam)
            .Produces((int)HttpStatusCode.OK, typeof(ExamResponse))
            .ProducesValidationProblem()
            .WithTags(ExamModuleConstants.RouteTag.ExamManagement)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);

        routeBuilder
            .MapDelete(ExamModuleConstants.Route.DeleteExam, DeleteExam)
            .Produces((int)HttpStatusCode.OK, typeof(bool))
            .ProducesValidationProblem()
            .WithTags(ExamModuleConstants.RouteTag.ExamManagement)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);

        routeBuilder
            .MapPut(ExamModuleConstants.Route.PublishExam, PublishExam)
            .Produces((int)HttpStatusCode.OK, typeof(bool))
            .WithTags(ExamModuleConstants.RouteTag.ExamManagement)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);

        routeBuilder
            .MapPut(ExamModuleConstants.Route.UnpublishExam, UnpublishExam)
            .Produces((int)HttpStatusCode.OK, typeof(bool))
            .WithTags(ExamModuleConstants.RouteTag.ExamManagement)
            .RequireAuthorization(AuthorizationPolicyConstants.QuizAuthorPolicy);
    }

    private static async Task<IResult> GetAllExams(ISender sender, int pageNumber = 1, int pageSize = 10)
    {
        var result = await sender.Send(new GetAllExamsQuery(pageNumber, pageSize));
        return result.ConvertToResult();
    }

    private static async Task<IResult> GetExamById(ISender sender, long examId)
    {
        var result = await sender.Send(new GetExamByIdQuery(examId));
        return result.ConvertToResult();
    }

    private static async Task<IResult> CreateExam(ISender sender, CreateExamCommand command)
    {
        var result = await sender.Send(command);
        return result.ConvertToResult();
    }

    private static async Task<IResult> UpdateExam(ISender sender, long examId, UpdateExamCommand command)
    {
        if (examId != command.ExamId)
            return Results.BadRequest("Invalid request");

        var result = await sender.Send(command);
        return result.ConvertToResult();
    }

    private static async Task<IResult> DeleteExam(ISender sender, long examId)
    {
        var result = await sender.Send(new DeleteExamCommand(examId));
        return result.ConvertToResult();
    }

    private static async Task<IResult> PublishExam(ISender sender, long examId)
    {
        var result = await sender.Send(new PublishExamCommand(examId));
        return result.ConvertToResult();
    }

    private static async Task<IResult> UnpublishExam(ISender sender, long examId)
    {
        var result = await sender.Send(new UnpublishExamCommand(examId));
        return result.ConvertToResult();
    }
}
