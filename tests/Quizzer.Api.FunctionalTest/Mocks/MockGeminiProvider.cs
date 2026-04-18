using Modules.AI.Core.Providers;
using Shared.Core;

namespace Quizzer.Api.FunctionalTest.Mocks;

public class MockGeminiProvider : IAIProvider
{
    public string ProviderId => "gemini";

    public Task<Result<string>> GenerateAsync(string systemPrompt, string userPrompt, string decryptedApiKey, CancellationToken cancellationToken = default)
    {
        // Detect if this is an interview prep request
        var isInterviewPrep = (systemPrompt?.Contains("interview", StringComparison.OrdinalIgnoreCase) ?? false) ||
                              (userPrompt?.Contains("interview", StringComparison.OrdinalIgnoreCase) ?? false);

        var json = isInterviewPrep ? GetInterviewPrepJson() : GetQuestionSetJson();
        Result<string> result = json;
        return Task.FromResult(result);
    }

    private static string GetQuestionSetJson()
    {
        return """
        {
            "title": "Test Question Set",
            "source": "topic",
            "complexity": "beginner",
            "experienceYears": null,
            "expertiseFields": [],
            "topics": ["Testing"],
            "totalQuestions": 10,
            "questions": [
                {
                    "sequence": 1,
                    "text": "What is unit testing?",
                    "type": "multiple_choice",
                    "options": [
                        {"id": "a", "text": "Testing individual units"},
                        {"id": "b", "text": "Testing the whole system"},
                        {"id": "c", "text": "Testing UI only"},
                        {"id": "d", "text": "Testing database only"}
                    ],
                    "correctOptionId": "a",
                    "explanation": "Unit testing tests individual units of code.",
                    "tags": ["testing"],
                    "difficultyScore": 3
                },
                {
                    "sequence": 2,
                    "text": "What is integration testing?",
                    "type": "multiple_choice",
                    "options": [
                        {"id": "a", "text": "Testing individual functions"},
                        {"id": "b", "text": "Testing how multiple components work together"},
                        {"id": "c", "text": "Testing only the database"},
                        {"id": "d", "text": "Testing the UI components"}
                    ],
                    "correctOptionId": "b",
                    "explanation": "Integration testing verifies that different modules work together correctly.",
                    "tags": ["testing"],
                    "difficultyScore": 4
                },
                {
                    "sequence": 3,
                    "text": "What is test-driven development (TDD)?",
                    "type": "multiple_choice",
                    "options": [
                        {"id": "a", "text": "Writing tests after code is complete"},
                        {"id": "b", "text": "Writing tests before writing the code"},
                        {"id": "c", "text": "Never writing tests"},
                        {"id": "d", "text": "Testing only the UI"}
                    ],
                    "correctOptionId": "b",
                    "explanation": "TDD involves writing tests first, then implementing code to pass those tests.",
                    "tags": ["testing", "methodology"],
                    "difficultyScore": 5
                },
                {
                    "sequence": 4,
                    "text": "What is mocking in testing?",
                    "type": "multiple_choice",
                    "options": [
                        {"id": "a", "text": "Making fun of the code"},
                        {"id": "b", "text": "Creating fake objects to simulate dependencies"},
                        {"id": "c", "text": "Testing with real databases"},
                        {"id": "d", "text": "Deleting test files"}
                    ],
                    "correctOptionId": "b",
                    "explanation": "Mocking creates fake objects that simulate real dependencies for isolated testing.",
                    "tags": ["testing", "techniques"],
                    "difficultyScore": 4
                },
                {
                    "sequence": 5,
                    "text": "What is a test fixture?",
                    "type": "multiple_choice",
                    "options": [
                        {"id": "a", "text": "A permanent test setup"},
                        {"id": "b", "text": "Initial state and data prepared for tests"},
                        {"id": "c", "text": "The final test results"},
                        {"id": "d", "text": "A test runner tool"}
                    ],
                    "correctOptionId": "b",
                    "explanation": "A test fixture is the initial state and data set up for running tests.",
                    "tags": ["testing"],
                    "difficultyScore": 3
                },
                {
                    "sequence": 6,
                    "text": "What is code coverage in testing?",
                    "type": "multiple_choice",
                    "options": [
                        {"id": "a", "text": "The cost of testing"},
                        {"id": "b", "text": "The percentage of code executed by tests"},
                        {"id": "c", "text": "The time taken to run tests"},
                        {"id": "d", "text": "The number of bugs found"}
                    ],
                    "correctOptionId": "b",
                    "explanation": "Code coverage measures what percentage of your code is executed during testing.",
                    "tags": ["testing", "metrics"],
                    "difficultyScore": 4
                },
                {
                    "sequence": 7,
                    "text": "What is continuous integration (CI)?",
                    "type": "multiple_choice",
                    "options": [
                        {"id": "a", "text": "Continuously changing the code"},
                        {"id": "b", "text": "Frequently integrating code changes and running automated tests"},
                        {"id": "c", "text": "Testing manually every day"},
                        {"id": "d", "text": "Using the same code forever"}
                    ],
                    "correctOptionId": "b",
                    "explanation": "CI is the practice of frequently merging code changes and running automated tests.",
                    "tags": ["testing", "devops"],
                    "difficultyScore": 4
                },
                {
                    "sequence": 8,
                    "text": "What is regression testing?",
                    "type": "multiple_choice",
                    "options": [
                        {"id": "a", "text": "Going back to older code"},
                        {"id": "b", "text": "Testing that new changes don't break existing functionality"},
                        {"id": "c", "text": "Testing only new features"},
                        {"id": "d", "text": "Removing old tests"}
                    ],
                    "correctOptionId": "b",
                    "explanation": "Regression testing ensures that new changes don't break previously working functionality.",
                    "tags": ["testing"],
                    "difficultyScore": 4
                },
                {
                    "sequence": 9,
                    "text": "What are assertions in testing?",
                    "type": "multiple_choice",
                    "options": [
                        {"id": "a", "text": "Statements declaring code ownership"},
                        {"id": "b", "text": "Statements that verify expected outcomes"},
                        {"id": "c", "text": "Database transactions"},
                        {"id": "d", "text": "API endpoints"}
                    ],
                    "correctOptionId": "b",
                    "explanation": "Assertions are statements that verify the expected outcome of a test.",
                    "tags": ["testing"],
                    "difficultyScore": 3
                },
                {
                    "sequence": 10,
                    "text": "What is end-to-end (E2E) testing?",
                    "type": "multiple_choice",
                    "options": [
                        {"id": "a", "text": "Testing from database to API only"},
                        {"id": "b", "text": "Testing the entire application flow from user perspective"},
                        {"id": "c", "text": "Testing only the user interface"},
                        {"id": "d", "text": "Testing individual functions"}
                    ],
                    "correctOptionId": "b",
                    "explanation": "E2E testing simulates real user scenarios and tests the entire application flow.",
                    "tags": ["testing"],
                    "difficultyScore": 5
                }
            ]
        }
        """;
    }

    private static string GetInterviewPrepJson()
    {
        return """
        {
            "jobTitle": "Senior Software Engineer",
            "keyTopics": [
                "System Design",
                "Data Structures and Algorithms",
                "Object-Oriented Design",
                "Database Design",
                "API Design"
            ],
            "readingMaterials": [
                {
                    "title": "Designing Data-Intensive Applications",
                    "description": "Comprehensive guide to distributed systems and scalable architecture",
                    "url": "https://example.com/books/ddia",
                    "type": "book"
                },
                {
                    "title": "System Design Interview Preparation",
                    "description": "Article covering common system design patterns and techniques",
                    "url": "https://example.com/articles/system-design",
                    "type": "article"
                }
            ],
            "practiceQuestions": [
                {
                    "question": "Design a URL shortening service like bit.ly",
                    "hint": "Consider load balancing, database sharding, and cache strategies"
                },
                {
                    "question": "Explain the difference between SQL and NoSQL databases",
                    "hint": "Think about consistency, scalability, and use cases"
                },
                {
                    "question": "How would you optimize a slow API endpoint?",
                    "hint": "Consider caching, database indexing, and query optimization"
                }
            ],
            "preparationTips": [
                "Practice explaining your thought process clearly and concisely",
                "Focus on trade-offs and why you chose specific approaches",
                "Review common design patterns and their applications",
                "Be prepared to discuss scalability and performance considerations",
                "Have questions ready to ask about the team and company culture"
            ]
        }
        """;
    }

    public Task<Result<bool>> TestConnectionAsync(string decryptedApiKey, CancellationToken cancellationToken = default)
    {
        Result<bool> result = true;
        return Task.FromResult(result);
    }
}
