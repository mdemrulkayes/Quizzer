namespace Modules.AI.Core.Models;

public class AIProviderConfig
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ProviderId { get; set; } = default!;
    public string ProviderName { get; set; } = default!;
    public string EncryptedSecretKey { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTimeOffset ConfiguredAt { get; set; }
    public DateTimeOffset? LastTestedAt { get; set; }
    public string? LastTestResult { get; set; }
}
