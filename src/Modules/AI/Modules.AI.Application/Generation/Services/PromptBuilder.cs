namespace Modules.AI.Application.Generation.Services;

public static class PromptBuilder
{
    public static (string SystemPrompt, string UserPrompt) BuildTopicQuestionSetPrompt(
        List<string> topics,
        string complexity,
        int questionCount,
        int? experienceYears = null,
        List<string>? expertiseFields = null)
    {
        var systemPrompt = @"You are an expert quiz generator. Your task is to generate a question set in strict JSON format.

RULES:
- Return ONLY valid JSON. No markdown, no explanation, no code fences, no wrapping text.
- Every question must test a distinct concept — do not rephrase or repeat questions.
- Follow the exact JSON schema below.

JSON SCHEMA:
{
  ""title"": ""string — descriptive title for the question set"",
  ""source"": ""topic"",
  ""complexity"": ""beginner | intermediate | professional | expert"",
  ""experienceYears"": number | null,
  ""expertiseFields"": [""string""],
  ""topics"": [""string""],
  ""totalQuestions"": number,
  ""questions"": [
    {
      ""sequence"": number,
      ""text"": ""string — the question text"",
      ""type"": ""multiple_choice | true_false | short_answer"",
      ""options"": [
        { ""id"": ""a"", ""text"": ""string"" },
        { ""id"": ""b"", ""text"": ""string"" },
        { ""id"": ""c"", ""text"": ""string"" },
        { ""id"": ""d"", ""text"": ""string"" }
      ],
      ""correctOptionId"": ""string | null (null for short_answer)"",
      ""explanation"": ""string — brief explanation of the correct answer"",
      ""tags"": [""string""],
      ""difficultyScore"": number (1-10)
    }
  ]
}

NOTES:
- For true_false questions, provide exactly 2 options: {""id"":""a"",""text"":""True""} and {""id"":""b"",""text"":""False""}
- For short_answer questions, options array should be empty and correctOptionId should be null
- For multiple_choice questions, provide exactly 4 options (a, b, c, d)
- Ensure questions cover different aspects of the topics
- Difficulty scores should vary and match the specified complexity level";

        var userPromptParts = new List<string>
        {
            $"Generate exactly {questionCount} questions on the following topics: {string.Join(", ", topics)}.",
            $"Complexity level: {complexity}."
        };

        if (experienceYears.HasValue)
            userPromptParts.Add($"Target audience has {experienceYears.Value} years of experience.");

        if (expertiseFields?.Count > 0)
            userPromptParts.Add($"Expertise areas: {string.Join(", ", expertiseFields)}.");

        userPromptParts.Add("Return ONLY the JSON object. No other text.");

        return (systemPrompt, string.Join("\n", userPromptParts));
    }

    public static (string SystemPrompt, string UserPrompt) BuildJobDescriptionQuestionSetPrompt(
        string jobTitle,
        string jobDescription,
        int questionCount)
    {
        var systemPrompt = @"You are an expert quiz generator specializing in job-specific assessments. Your task is to generate a question set based on a job description in strict JSON format.

RULES:
- Return ONLY valid JSON. No markdown, no explanation, no code fences, no wrapping text.
- Questions should test the skills, knowledge, and competencies mentioned in the job description.
- Every question must test a distinct concept — do not rephrase or repeat questions.
- Include both technical and situational/behavioral questions when appropriate.
- Follow the exact JSON schema below.

JSON SCHEMA:
{
  ""title"": ""string — descriptive title for the question set"",
  ""source"": ""job_description"",
  ""complexity"": ""intermediate"",
  ""experienceYears"": null,
  ""expertiseFields"": [""string — extracted from job description""],
  ""topics"": [""string — key skills/topics from job description""],
  ""totalQuestions"": number,
  ""questions"": [
    {
      ""sequence"": number,
      ""text"": ""string — the question text"",
      ""type"": ""multiple_choice | true_false | short_answer"",
      ""options"": [
        { ""id"": ""a"", ""text"": ""string"" },
        { ""id"": ""b"", ""text"": ""string"" },
        { ""id"": ""c"", ""text"": ""string"" },
        { ""id"": ""d"", ""text"": ""string"" }
      ],
      ""correctOptionId"": ""string | null (null for short_answer)"",
      ""explanation"": ""string — brief explanation of the correct answer"",
      ""tags"": [""string""],
      ""difficultyScore"": number (1-10)
    }
  ]
}

NOTES:
- For true_false questions, provide exactly 2 options
- For short_answer questions, options array should be empty and correctOptionId should be null
- For multiple_choice questions, provide exactly 4 options (a, b, c, d)
- Infer the complexity from the job description seniority level
- Extract relevant topics and expertise fields from the job description";

        var userPrompt = $@"Generate exactly {questionCount} questions for the following job:

Job Title: {jobTitle}

Job Description:
{jobDescription}

Return ONLY the JSON object. No other text.";

        return (systemPrompt, userPrompt);
    }

    public static (string SystemPrompt, string UserPrompt) BuildInterviewPrepPrompt(
        string jobTitle,
        string jobDescription)
    {
        var systemPrompt = @"You are an expert career coach and interview preparation specialist. Your task is to generate comprehensive interview preparation material based on a job description in strict JSON format.

RULES:
- Return ONLY valid JSON. No markdown, no explanation, no code fences, no wrapping text.
- Provide practical, actionable preparation advice.
- Include a mix of technical and behavioral preparation material.
- Follow the exact JSON schema below.

JSON SCHEMA:
{
  ""jobTitle"": ""string"",
  ""keyTopics"": [""string — key topics to study for this role""],
  ""readingMaterials"": [
    {
      ""title"": ""string"",
      ""description"": ""string — why this is relevant"",
      ""url"": ""string | null"",
      ""type"": ""article | book | video | documentation | course""
    }
  ],
  ""practiceQuestions"": [
    {
      ""question"": ""string — open-ended practice question"",
      ""hint"": ""string — guidance on how to approach this question""
    }
  ],
  ""preparationTips"": [""string — actionable preparation tips""]
}

NOTES:
- Include 5-10 key topics
- Include 5-8 reading materials with varied types
- Include 8-12 practice questions covering technical and behavioral aspects
- Include 5-8 preparation tips
- Reading material URLs should be real, well-known resources when possible, or null if unsure";

        var userPrompt = $@"Generate interview preparation material for the following job:

Job Title: {jobTitle}

Job Description:
{jobDescription}

Return ONLY the JSON object. No other text.";

        return (systemPrompt, userPrompt);
    }
}
