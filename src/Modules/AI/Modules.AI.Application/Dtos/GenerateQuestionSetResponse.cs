namespace Modules.AI.Application.Dtos;

public sealed record GenerateQuestionSetResponse(
    Guid GenerationRequestId,
    string Title,
    int QuestionCount,
    string Status);
