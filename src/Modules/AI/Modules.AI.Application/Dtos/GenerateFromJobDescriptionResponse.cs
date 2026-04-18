namespace Modules.AI.Application.Dtos;

public sealed record GenerateFromJobDescriptionResponse(
    Guid GenerationRequestId,
    string OutputType,
    string Title,
    string Status);
