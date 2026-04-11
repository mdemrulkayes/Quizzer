import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ExamResponse,
  CreateExamRequest,
  UpdateExamRequest,
  ExamAttemptStartResponse,
  SubmitAnswerRequest,
  ExamSubmitResponse,
  ExamResultResponse,
  ExamAttemptResponse,
} from '../models';
import { PaginatedList } from '../models';

@Injectable({ providedIn: 'root' })
export class ExamService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/exam`;

  // Exam Management
  getExams(pageNumber = 1, pageSize = 10): Observable<PaginatedList<ExamResponse>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<PaginatedList<ExamResponse>>(this.baseUrl, { params });
  }

  getExamById(examId: number): Observable<ExamResponse> {
    return this.http.get<ExamResponse>(`${this.baseUrl}/${examId}`);
  }

  createExam(request: CreateExamRequest): Observable<ExamResponse> {
    return this.http.post<ExamResponse>(this.baseUrl, request);
  }

  updateExam(examId: number, request: UpdateExamRequest): Observable<ExamResponse> {
    return this.http.put<ExamResponse>(`${this.baseUrl}/${examId}`, request);
  }

  deleteExam(examId: number): Observable<boolean> {
    return this.http.delete<boolean>(`${this.baseUrl}/${examId}`);
  }

  publishExam(examId: number): Observable<boolean> {
    return this.http.put<boolean>(`${this.baseUrl}/${examId}/publish`, {});
  }

  unpublishExam(examId: number): Observable<boolean> {
    return this.http.put<boolean>(`${this.baseUrl}/${examId}/unpublish`, {});
  }

  // Exam Taking
  getAvailableExams(pageNumber = 1, pageSize = 10): Observable<PaginatedList<ExamResponse>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<PaginatedList<ExamResponse>>(`${this.baseUrl}/available`, { params });
  }

  startExam(examId: number): Observable<ExamAttemptStartResponse> {
    return this.http.post<ExamAttemptStartResponse>(`${this.baseUrl}/${examId}/start`, {});
  }

  submitAnswer(examId: number, request: Omit<SubmitAnswerRequest, 'examId'>): Observable<boolean> {
    return this.http.post<boolean>(`${this.baseUrl}/${examId}/answer`, request);
  }

  submitExam(examId: number): Observable<ExamSubmitResponse> {
    return this.http.post<ExamSubmitResponse>(`${this.baseUrl}/${examId}/submit`, {});
  }

  // Exam Results
  getMyExamResult(examId: number): Observable<ExamResultResponse> {
    return this.http.get<ExamResultResponse>(`${this.baseUrl}/${examId}/result`);
  }

  getExamResults(examId: number, pageNumber = 1, pageSize = 10): Observable<PaginatedList<ExamAttemptResponse>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<PaginatedList<ExamAttemptResponse>>(`${this.baseUrl}/${examId}/results`, { params });
  }

  getMyAllResults(pageNumber = 1, pageSize = 10): Observable<PaginatedList<ExamAttemptResponse>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<PaginatedList<ExamAttemptResponse>>(`${this.baseUrl}/my-results`, { params });
  }
}
