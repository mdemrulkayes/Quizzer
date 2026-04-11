export interface QuestionResponse {
  questionId: number;
  question: string;
  details: string;
  mark: number | null;
  questionOptions: QuestionOptionResponse[];
}

export interface QuestionOptionResponse {
  questionOptionId: number;
  optionText: string;
  isCorrect: boolean;
}

export interface QuestionSetResponse {
  questionSetId: number;
  name: string;
  setCode: string | null;
  details: string | null;
  questions: QuestionResponse[];
}

export interface TagResponse {
  tagId: number;
  name: string;
  description: string | null;
}

export interface CreateQuestionRequest {
  question: string;
  details: string;
  mark: number | null;
  questionOptions: CreateQuestionOptionRequest[];
}

export interface CreateQuestionOptionRequest {
  optionText: string;
  isAnswer: boolean;
}

export interface UpdateQuestionRequest {
  questionId: number;
  question: string;
  details: string;
  mark: number | null;
}

export interface CreateQuestionSetRequest {
  name: string;
  setCode: string | null;
  details: string | null;
  questions: CreateQuestionRequest[];
}

export interface UpdateQuestionSetRequest {
  questionSetId: number;
  name: string;
  setCode: string | null;
  details: string | null;
}

export interface CreateTagRequest {
  name: string;
  description: string | null;
}

export interface UpdateTagRequest {
  tagId: number;
  name: string;
  description: string;
}

export interface AddOptionRequest {
  optionText: string;
  isAnswer: boolean;
}

export interface UpdateOptionRequest {
  optionText: string;
  isAnswer: boolean;
}

export interface AssignTagRequest {
  tagId: number;
}
