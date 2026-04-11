namespace Modules.Exam.Core.ExamAggregate;

public static class ExamModuleConstants
{
    public const string SchemaName = "Exam";
    public const string MigrationHistoryTableName = "__ExamModuleMigrationHistory";

    public static class Route
    {
        public const string CreateExam = "/api/exam";
        public const string GetAllExams = "/api/exam";
        public const string GetExamById = "/api/exam/{examId}";
        public const string UpdateExam = "/api/exam/{examId}";
        public const string DeleteExam = "/api/exam/{examId}";
        public const string PublishExam = "/api/exam/{examId}/publish";
        public const string UnpublishExam = "/api/exam/{examId}/unpublish";
        public const string GetAvailableExams = "/api/exam/available";
        public const string StartExam = "/api/exam/{examId}/start";
        public const string SubmitAnswer = "/api/exam/{examId}/answer";
        public const string SubmitExam = "/api/exam/{examId}/submit";
        public const string GetMyResult = "/api/exam/{examId}/result";
        public const string GetExamResults = "/api/exam/{examId}/results";
        public const string GetMyAllResults = "/api/exam/my-results";
    }

    public static class RouteTag
    {
        public const string ExamManagement = "Exam Management";
        public const string ExamTaking = "Exam Taking";
        public const string ExamResults = "Exam Results";
    }
}
