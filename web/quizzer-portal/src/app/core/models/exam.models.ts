export interface ExamResponse {
  examId: number;
  title: string;
  description: string | null;
  questionSetId: number;
  durationInMinutes: number;
  totalMarks: number;
  passingMarks: number;
  isPublished: boolean;
  scheduledStartTime: string | null;
  scheduledEndTime: string | null;
}

export interface CreateExamRequest {
  title: string;
  description: string | null;
  questionSetId: number;
  durationInMinutes: number;
  totalMarks: number;
  passingMarks: number;
  scheduledStartTime: string | null;
  scheduledEndTime: string | null;
}

export interface UpdateExamRequest {
  examId: number;
  title: string;
  description: string | null;
  durationInMinutes: number;
  totalMarks: number;
  passingMarks: number;
  scheduledStartTime: string | null;
  scheduledEndTime: string | null;
}

export interface ExamAttemptStartResponse {
  examAttemptId: number;
  examId: number;
  examTitle: string;
  durationInMinutes: number;
  startedAt: string;
  expiresAt: string;
  questions: ExamQuestionResponse[];
}

export interface ExamQuestionResponse {
  questionId: number;
  questionText: string;
  marks: number | null;
  options: ExamQuestionOptionResponse[];
}

export interface ExamQuestionOptionResponse {
  optionId: number;
  optionText: string;
}

export interface SubmitAnswerRequest {
  examId: number;
  questionId: number;
  selectedOptionId: number | null;
}

export interface ExamSubmitResponse {
  examAttemptId: number;
  totalScore: number;
  totalMarks: number;
  passingMarks: number;
  isPassed: boolean;
  status: string;
}

export interface ExamAttemptResponse {
  examAttemptId: number;
  examId: number;
  examTitle: string;
  userId: string;
  startedAt: string;
  submittedAt: string | null;
  status: ExamAttemptStatus;
  totalScore: number | null;
  isPassed: boolean | null;
}

export enum ExamAttemptStatus {
  InProgress = 'InProgress',
  Submitted = 'Submitted',
  TimedOut = 'TimedOut',
  Graded = 'Graded',
  Cancelled = 'Cancelled',
}

export interface ExamResultResponse {
  examAttemptId: number;
  examId: number;
  examTitle: string;
  startedAt: string;
  submittedAt: string | null;
  status: string;
  totalScore: number | null;
  totalMarks: number;
  passingMarks: number;
  isPassed: boolean | null;
  answers: AnswerDetailResponse[];
}

export interface AnswerDetailResponse {
  questionId: number;
  questionText: string;
  selectedOptionId: number | null;
  selectedOptionText: string | null;
  isCorrect: boolean | null;
  marksAwarded: number | null;
}
