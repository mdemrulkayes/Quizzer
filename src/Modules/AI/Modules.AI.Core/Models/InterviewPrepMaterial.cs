namespace Modules.AI.Core.Models;

public class InterviewPrepMaterial
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string JobTitle { get; set; } = default!;
    public string JobDescription { get; set; } = default!;
    public List<string> KeyTopics { get; set; } = new();
    public List<ReadingMaterial> ReadingMaterials { get; set; } = new();
    public List<PracticeQuestion> PracticeQuestions { get; set; } = new();
    public List<string> PreparationTips { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
}

public class ReadingMaterial
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Url { get; set; }
    public string Type { get; set; } = default!;
}

public class PracticeQuestion
{
    public string Question { get; set; } = default!;
    public string Hint { get; set; } = default!;
}
