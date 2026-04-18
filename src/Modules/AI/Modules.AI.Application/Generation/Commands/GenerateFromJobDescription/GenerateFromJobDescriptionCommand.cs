using Modules.AI.Application.Dtos;
using Shared.Core;

namespace Modules.AI.Application.Generation.Commands.GenerateFromJobDescription;

public sealed record GenerateFromJobDescriptionCommand(
    string JobTitle,
    string JobDescription,
    string OutputType,
    int QuestionCount) : ICommand<Result<GenerateFromJobDescriptionResponse>>;
