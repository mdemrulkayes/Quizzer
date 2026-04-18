using Modules.AI.Application.Dtos;
using Shared.Core;

namespace Modules.AI.Application.InterviewPrep.Queries.GetInterviewPrepMaterials;

public sealed record GetInterviewPrepMaterialsQuery(int PageNumber = 1, int PageSize = 10)
    : IQuery<Result<List<InterviewPrepMaterialDto>>>;
