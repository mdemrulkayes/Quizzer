using Modules.AI.Core.Enums;

namespace Modules.AI.Core.Models;

public class AIGenerationRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public GenerationSource Source { get; set; }
    public GenerationOutputType OutputType { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public GenerationStatus Status { get; set; }
    public string? RawAIResponse { get; set; }
    public string? ErrorMessage { get; set; }
    public long? ResultQuestionSetId { get; set; }
    public Guid? ResultInterviewPrepId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
