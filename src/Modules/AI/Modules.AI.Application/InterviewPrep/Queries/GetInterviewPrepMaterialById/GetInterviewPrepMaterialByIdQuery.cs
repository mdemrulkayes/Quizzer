using Modules.AI.Application.Dtos;
using Shared.Core;

namespace Modules.AI.Application.InterviewPrep.Queries.GetInterviewPrepMaterialById;

public sealed record GetInterviewPrepMaterialByIdQuery(Guid Id)
    : IQuery<Result<InterviewPrepMaterialDetailDto>>;
