using Modules.AI.Application.Dtos;
using Shared.Core;

namespace Modules.AI.Application.Generation.Commands.GenerateQuestionSet;

public sealed record GenerateQuestionSetCommand(
    List<string> Topics,
    string Complexity,
    int QuestionCount,
    int? ExperienceYears,
    List<string>? ExpertiseFields) : ICommand<Result<GenerateQuestionSetResponse>>;
