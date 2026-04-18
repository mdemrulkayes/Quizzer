import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  SupportedProvider,
  ProviderConfigResponse,
  SaveProviderConfigRequest,
  TestConnectionResponse,
  GenerateQuestionSetRequest,
  GenerateQuestionSetResponse,
  GenerateFromJobDescriptionRequest,
  GenerateFromJobDescriptionResponse,
  GenerationHistoryItem,
  InterviewPrepMaterialSummary,
  InterviewPrepMaterialDetail,
} from '../models';

@Injectable({ providedIn: 'root' })
export class AIService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/ai`;

  // Provider Config
  getSupportedProviders(): Observable<SupportedProvider[]> {
    return this.http.get<SupportedProvider[]>(`${this.baseUrl}/providers/supported`);
  }

  getProviderConfig(): Observable<ProviderConfigResponse> {
    return this.http.get<ProviderConfigResponse>(`${this.baseUrl}/provider-config`);
  }

  saveProviderConfig(request: SaveProviderConfigRequest): Observable<ProviderConfigResponse> {
    return this.http.post<ProviderConfigResponse>(`${this.baseUrl}/provider-config`, request);
  }

  deleteProviderConfig(): Observable<boolean> {
    return this.http.delete<boolean>(`${this.baseUrl}/provider-config`);
  }

  testProviderConnection(): Observable<TestConnectionResponse> {
    return this.http.post<TestConnectionResponse>(`${this.baseUrl}/provider-config/test`, {});
  }

  // Generation
  generateQuestionSet(request: GenerateQuestionSetRequest): Observable<GenerateQuestionSetResponse> {
    return this.http.post<GenerateQuestionSetResponse>(`${this.baseUrl}/generate/question-set`, request);
  }

  generateFromJobDescription(request: GenerateFromJobDescriptionRequest): Observable<GenerateFromJobDescriptionResponse> {
    return this.http.post<GenerateFromJobDescriptionResponse>(`${this.baseUrl}/generate/from-job-description`, request);
  }

  getGenerationHistory(params?: { pageNumber?: number; pageSize?: number }): Observable<GenerationHistoryItem[]> {
    let httpParams = new HttpParams();
    if (params?.pageNumber) httpParams = httpParams.set('PageNumber', params.pageNumber);
    if (params?.pageSize) httpParams = httpParams.set('PageSize', params.pageSize);
    return this.http.get<GenerationHistoryItem[]>(`${this.baseUrl}/generation-history`, { params: httpParams });
  }

  // Interview Prep
  getInterviewPrepMaterials(params?: { pageNumber?: number; pageSize?: number }): Observable<InterviewPrepMaterialSummary[]> {
    let httpParams = new HttpParams();
    if (params?.pageNumber) httpParams = httpParams.set('PageNumber', params.pageNumber);
    if (params?.pageSize) httpParams = httpParams.set('PageSize', params.pageSize);
    return this.http.get<InterviewPrepMaterialSummary[]>(`${this.baseUrl}/interview-prep`, { params: httpParams });
  }

  getInterviewPrepMaterial(id: string): Observable<InterviewPrepMaterialDetail> {
    return this.http.get<InterviewPrepMaterialDetail>(`${this.baseUrl}/interview-prep/${id}`);
  }
}
