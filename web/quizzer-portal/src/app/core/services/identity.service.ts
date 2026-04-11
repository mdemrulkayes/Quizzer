import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { UserDetailResponse, UserListResponse, UpdateUserRoleRequest } from '../models';

@Injectable({ providedIn: 'root' })
export class IdentityService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/identity`;

  getUsers(pageNumber = 1, pageSize = 10): Observable<UserListResponse> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<UserListResponse>(`${this.baseUrl}/users`, { params });
  }

  getUserById(userId: string): Observable<UserDetailResponse> {
    return this.http.get<UserDetailResponse>(`${this.baseUrl}/users/${userId}`);
  }

  updateUserRole(userId: string, request: UpdateUserRoleRequest): Observable<any> {
    return this.http.put(`${this.baseUrl}/users/${userId}/role`, request);
  }

  deleteUser(userId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/users/${userId}`);
  }
}
