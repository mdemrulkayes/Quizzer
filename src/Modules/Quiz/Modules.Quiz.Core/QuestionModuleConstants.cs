namespace Modules.Quiz.Core;
public struct QuestionModuleConstants
{
    public const string SchemaName = "Question";
    public static string MigrationHistoryTableName = "__QuestionModuleMigrationHistory";

    public struct Route
    {
        public struct TagRoute
        {
            public const string GetAllTags = "/api/question/tag";
            public const string GetTagDetailsById = "/api/question/tag/{tagId}";
            public const string CreateTag = "/api/question/tag";
            public const string UpdateTag = "/api/question/tag/{tagId}";
            public const string DeleteTag = "/api/question/tag/{tagId}";
        }

        public struct QuestionSetRoute
        {
            public const string GetAllQuestionSets = "/api/question/questionSet";
            public const string GetQuestionSetDetailsById = "/api/question/questionSet/{setId}";
            public const string CreateQuestionSet = "/api/question/questionSet";
            public const string UpdateQuestionSet = "/api/question/questionSet/{setId}";
            public const string DeleteQuestionSet = "/api/question/questionSet/{setId}";
            public const string GetQuestionSetTags = "/api/question/questionSet/{setId}/tags";
            public const string AssignTagToQuestionSet = "/api/question/questionSet/{setId}/tags";
            public const string RemoveTagFromQuestionSet = "/api/question/questionSet/{setId}/tags/{tagId}";
        }

        public struct QuestionRoute
        {
            public const string GetAllQuestions = "/api/question";
            public const string GetQuestionDetailsById = "/api/question/{questionId}";
            public const string CreateQuestion = "/api/question";
            public const string UpdateQuestion = "/api/question/{questionId}";
            public const string DeleteQuestion = "/api/question/{questionId}";
            public const string AddOption = "/api/question/{questionId}/options";
            public const string UpdateOption = "/api/question/{questionId}/options/{optionId}";
            public const string DeleteOption = "/api/question/{questionId}/options/{optionId}";
        }
    }

    public struct RouteTag
    {
        public const string TagEndPointTagName = "Tag";
        public const string QuestionSetTag = "QuestionSet";
        public const string QuestionTag = "Question";
    }
}
