namespace Modules.AI.Core;

public static class AIModuleConstants
{
    public const string ModuleName = "AI";

    public static class Route
    {
        private const string BaseRoute = "/api/ai";

        public static class ProviderConfig
        {
            public const string GetSupportedProviders = $"{BaseRoute}/providers/supported";
            public const string SaveProviderConfig = $"{BaseRoute}/provider-config";
            public const string GetProviderConfig = $"{BaseRoute}/provider-config";
            public const string DeleteProviderConfig = $"{BaseRoute}/provider-config";
            public const string TestProviderConnection = $"{BaseRoute}/provider-config/test";
        }

        public static class Generation
        {
            public const string GenerateQuestionSet = $"{BaseRoute}/generate/question-set";
            public const string GenerateFromJobDescription = $"{BaseRoute}/generate/from-job-description";
            public const string GetGenerationHistory = $"{BaseRoute}/generation-history";
        }

        public static class InterviewPrep
        {
            public const string GetAll = $"{BaseRoute}/interview-prep";
            public const string GetById = $"{BaseRoute}/interview-prep/{{id}}";
        }
    }

    public static class RouteTag
    {
        public const string ProviderConfigTag = "AI Provider Config";
        public const string GenerationTag = "AI Generation";
        public const string InterviewPrepTag = "AI Interview Prep";
    }
}
