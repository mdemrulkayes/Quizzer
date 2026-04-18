import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  QuestionResponse,
  QuestionSetResponse,
  TagResponse,
  CreateQuestionRequest,
  UpdateQuestionRequest,
  CreateQuestionSetRequest,
  UpdateQuestionSetRequest,
  CreateTagRequest,
  UpdateTagRequest,
  AddOptionRequest,
  UpdateOptionRequest,
  AssignTagRequest,
} from '../models';
import { PaginatedList } from '../models';

@Injectable({ providedIn: 'root' })
export class QuizService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/question`;

  // Questions
  getQuestions(params?: { searchText?: string; questionSetId?: number; pageNumber?: number; pageSize?: number }): Observable<PaginatedList<QuestionResponse>> {
    let httpParams = new HttpParams();
    if (params?.searchText) httpParams = httpParams.set('SearchText', params.searchText);
    if (params?.questionSetId) httpParams = httpParams.set('QuestionSetId', params.questionSetId);
    if (params?.pageNumber) httpParams = httpParams.set('PageNumber', params.pageNumber);
    if (params?.pageSize) httpParams = httpParams.set('PageSize', params.pageSize);
    return this.http.get<PaginatedList<QuestionResponse>>(this.baseUrl, { params: httpParams });
  }

  getQuestionById(questionId: number): Observable<QuestionResponse> {
    return this.http.get<QuestionResponse>(`${this.baseUrl}/${questionId}`);
  }

  createQuestion(request: CreateQuestionRequest): Observable<QuestionResponse> {
    return this.http.post<QuestionResponse>(this.baseUrl, request);
  }

  updateQuestion(questionId: number, request: UpdateQuestionRequest): Observable<QuestionResponse> {
    return this.http.put<QuestionResponse>(`${this.baseUrl}/${questionId}`, request);
  }

  deleteQuestion(questionId: number): Observable<boolean> {
    return this.http.delete<boolean>(`${this.baseUrl}/${questionId}`);
  }

  // Question Options
  addOption(questionId: number, request: AddOptionRequest): Observable<any> {
    return this.http.post(`${this.baseUrl}/${questionId}/options`, request);
  }

  updateOption(questionId: number, optionId: number, request: UpdateOptionRequest): Observable<any> {
    return this.http.put(`${this.baseUrl}/${questionId}/options/${optionId}`, request);
  }

  deleteOption(questionId: number, optionId: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${questionId}/options/${optionId}`);
  }

  // Question Sets
  getQuestionSets(params?: { searchName?: string; tagId?: number; sortBy?: string; pageNumber?: number; pageSize?: number }): Observable<PaginatedList<QuestionSetResponse>> {
    let httpParams = new HttpParams();
    if (params?.searchName) httpParams = httpParams.set('SearchName', params.searchName);
    if (params?.tagId) httpParams = httpParams.set('TagId', params.tagId);
    if (params?.sortBy) httpParams = httpParams.set('SortBy', params.sortBy);
    if (params?.pageNumber) httpParams = httpParams.set('PageNumber', params.pageNumber);
    if (params?.pageSize) httpParams = httpParams.set('PageSize', params.pageSize);
    return this.http.get<PaginatedList<QuestionSetResponse>>(`${this.baseUrl}/questionSet`, { params: httpParams });
  }

  getQuestionSetById(setId: number): Observable<QuestionSetResponse> {
    return this.http.get<QuestionSetResponse>(`${this.baseUrl}/questionSet/${setId}`);
  }

  createQuestionSet(request: CreateQuestionSetRequest): Observable<QuestionSetResponse> {
    return this.http.post<QuestionSetResponse>(`${this.baseUrl}/questionSet`, request);
  }

  updateQuestionSet(setId: number, request: UpdateQuestionSetRequest): Observable<QuestionSetResponse> {
    return this.http.put<QuestionSetResponse>(`${this.baseUrl}/questionSet/${setId}`, request);
  }

  deleteQuestionSet(setId: number): Observable<boolean> {
    return this.http.delete<boolean>(`${this.baseUrl}/questionSet/${setId}`);
  }

  toggleVisibility(setId: number, isPublic: boolean): Observable<QuestionSetResponse> {
    return this.http.patch<QuestionSetResponse>(
      `${this.baseUrl}/questionSet/${setId}/visibility`,
      { questionSetId: setId, isPublic }
    );
  }

  // Question Set Tags
  getQuestionSetTags(setId: number): Observable<TagResponse[]> {
    return this.http.get<TagResponse[]>(`${this.baseUrl}/questionSet/${setId}/tags`);
  }

  assignTagToQuestionSet(setId: number, request: AssignTagRequest): Observable<TagResponse> {
    return this.http.post<TagResponse>(`${this.baseUrl}/questionSet/${setId}/tags`, request);
  }

  removeTagFromQuestionSet(setId: number, tagId: number): Observable<boolean> {
    return this.http.delete<boolean>(`${this.baseUrl}/questionSet/${setId}/tags/${tagId}`);
  }

  // Tags
  getTags(params?: { searchName?: string; pageNumber?: number; pageSize?: number }): Observable<PaginatedList<TagResponse>> {
    let httpParams = new HttpParams();
    if (params?.searchName) httpParams = httpParams.set('SearchName', params.searchName);
    if (params?.pageNumber) httpParams = httpParams.set('PageNumber', params.pageNumber);
    if (params?.pageSize) httpParams = httpParams.set('PageSize', params.pageSize);
    return this.http.get<PaginatedList<TagResponse>>(`${this.baseUrl}/tag`, { params: httpParams });
  }

  getTagById(tagId: number): Observable<TagResponse> {
    return this.http.get<TagResponse>(`${this.baseUrl}/tag/${tagId}`);
  }

  createTag(request: CreateTagRequest): Observable<TagResponse> {
    return this.http.post<TagResponse>(`${this.baseUrl}/tag`, request);
  }

  updateTag(tagId: number, request: UpdateTagRequest): Observable<TagResponse> {
    return this.http.put<TagResponse>(`${this.baseUrl}/tag/${tagId}`, request);
  }

  deleteTag(tagId: number): Observable<boolean> {
    return this.http.delete<boolean>(`${this.baseUrl}/tag/${tagId}`);
  }
}
