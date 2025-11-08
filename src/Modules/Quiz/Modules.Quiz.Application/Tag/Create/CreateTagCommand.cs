using Modules.Quiz.Application.Tag.Dtos;
using Shared.Core;

namespace Modules.Quiz.Application.Tag.Create;

public sealed record CreateTagCommand(string Name, string? Description) : ICommand<Result<TagResponse>>;
