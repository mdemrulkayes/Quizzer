// Provider Config
export interface SupportedProvider {
  providerId: string;
  providerName: string;
  description: string;
  defaultModel: string;
}

export interface ProviderConfigResponse {
  id: string;
  providerId: string;
  providerName: string;
  isActive: boolean;
  maskedApiKey: string;
  configuredAt: string;
  lastTestedAt: string | null;
  lastTestResult: 'success' | 'failed' | null;
}

export interface SaveProviderConfigRequest {
  providerId: string;
  secretKey: string;
}

export interface TestConnectionResponse {
  success: boolean;
  message: string | null;
}

// Generation
export interface GenerateQuestionSetRequest {
  topics: string[];
  complexity: 'beginner' | 'intermediate' | 'professional' | 'expert';
  questionCount: number;
  experienceYears?: number;
  expertiseFields?: string[];
}

export interface GenerateFromJobDescriptionRequest {
  jobTitle: string;
  jobDescription: string;
  outputType: 'question_set' | 'interview_prep';
  questionCount: number;
}

export interface GenerateQuestionSetResponse {
  generationRequestId: string;
  title: string;
  questionCount: number;
  status: string;
}

export interface GenerateFromJobDescriptionResponse {
  generationRequestId: string;
  outputType: string;
  title: string;
  status: string;
}

// Generation History
export interface GenerationHistoryItem {
  id: string;
  source: string;
  outputType: string;
  status: string;
  errorMessage: string | null;
  createdAt: string;
  completedAt: string | null;
}

// Interview Prep
export interface InterviewPrepMaterialSummary {
  id: string;
  jobTitle: string;
  keyTopics: string[];
  createdAt: string;
}

export interface InterviewPrepMaterialDetail {
  id: string;
  jobTitle: string;
  jobDescription: string;
  keyTopics: string[];
  readingMaterials: ReadingMaterial[];
  practiceQuestions: PracticeQuestion[];
  preparationTips: string[];
  createdAt: string;
}

export interface ReadingMaterial {
  title: string;
  description: string;
  url: string | null;
  type: string;
}

export interface PracticeQuestion {
  question: string;
  hint: string;
}
