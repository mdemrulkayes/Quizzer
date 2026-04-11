import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of } from 'rxjs';
import {
  AccessTokenResponse,
  ChangePasswordRequest,
  LoginRequest,
  RefreshTokenRequest,
  RegisterRequest,
  UserProfile,
  UserRole,
  UserType,
} from '../models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly _accessToken = signal<string | null>(null);
  private readonly _refreshToken = signal<string | null>(localStorage.getItem('refreshToken'));
  private readonly _currentUser = signal<UserProfile | null>(null);
  private readonly _isInitialized = signal(false);

  readonly accessToken = this._accessToken.asReadonly();
  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => !!this._accessToken() && !!this._currentUser());
  readonly isInitialized = this._isInitialized.asReadonly();
  readonly userRoles = computed(() => this._currentUser()?.roles ?? []);

  hasRole(role: string): boolean {
    return this.userRoles().includes(role);
  }

  hasAnyRole(roles: string[]): boolean {
    return roles.some((role) => this.userRoles().includes(role));
  }

  get isSuperAdmin(): boolean {
    return this.hasRole(UserRole.SuperAdmin);
  }

  get isAdmin(): boolean {
    return this.hasAnyRole([UserRole.SuperAdmin, UserRole.SupportAdmin]);
  }

  get isQuizAuthor(): boolean {
    return this.hasAnyRole([UserRole.SuperAdmin, UserRole.SupportAdmin, UserRole.QuizAuthor]);
  }

  get isExaminee(): boolean {
    return this.hasRole(UserRole.Examine);
  }

  login(request: LoginRequest): Observable<AccessTokenResponse> {
    return this.http.post<AccessTokenResponse>(`${environment.apiBaseUrl}/identity/login`, request).pipe(
      tap((response) => this.handleAuthResponse(response)),
    );
  }

  register(request: RegisterRequest): Observable<any> {
    const payload = { ...request, userType: UserType.Examine };
    return this.http.post(`${environment.apiBaseUrl}/identity/register`, payload);
  }

  logout(): void {
    this._accessToken.set(null);
    this._refreshToken.set(null);
    this._currentUser.set(null);
    localStorage.removeItem('refreshToken');
    this.router.navigate(['/auth/login']);
  }

  refreshAccessToken(): Observable<AccessTokenResponse | null> {
    const currentToken = this._accessToken();
    const refreshToken = this._refreshToken();
    if (!currentToken || !refreshToken) {
      this.logout();
      return of(null);
    }
    const request: RefreshTokenRequest = {
      accessToken: currentToken,
      refreshToken,
    };
    return this.http
      .post<AccessTokenResponse>(`${environment.apiBaseUrl}/identity/refresh-token`, request)
      .pipe(
        tap((response) => this.handleAuthResponse(response)),
        catchError(() => {
          this.logout();
          return of(null);
        }),
      );
  }

  loadProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${environment.apiBaseUrl}/identity/profile`).pipe(
      tap((profile) => this._currentUser.set(profile)),
    );
  }

  changePassword(request: ChangePasswordRequest): Observable<any> {
    return this.http.put(`${environment.apiBaseUrl}/identity/change-password`, request);
  }

  tryRestoreSession(): Observable<boolean> {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) {
      this._isInitialized.set(true);
      return of(false);
    }
    // We don't have the access token in memory after refresh, so we can't restore.
    // User must log in again. But we try to use refresh token if we had one stored.
    this._isInitialized.set(true);
    return of(false);
  }

  private handleAuthResponse(response: AccessTokenResponse): void {
    this._accessToken.set(response.token);
    this._refreshToken.set(response.refreshToken);
    localStorage.setItem('refreshToken', response.refreshToken);
  }

  initialize(): void {
    this._isInitialized.set(true);
  }
}
