import { HttpErrorResponse, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { MessageService } from 'primeng/api';

let isRefreshing = false;

export const errorInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const messageService = inject(MessageService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !req.url.includes('/identity/login') && !req.url.includes('/identity/refresh-token')) {
        if (!isRefreshing) {
          isRefreshing = true;
          return authService.refreshAccessToken().pipe(
            switchMap((result) => {
              isRefreshing = false;
              if (result) {
                const clonedReq = req.clone({
                  headers: req.headers.set('Authorization', `Bearer ${result.token}`),
                });
                return next(clonedReq);
              }
              authService.logout();
              return throwError(() => error);
            }),
            catchError((refreshError) => {
              isRefreshing = false;
              authService.logout();
              return throwError(() => refreshError);
            }),
          );
        }
        authService.logout();
      } else if (error.status === 403) {
        messageService.add({
          severity: 'error',
          summary: 'Access Denied',
          detail: 'You do not have permission to perform this action.',
        });
      } else if (error.status >= 500) {
        messageService.add({
          severity: 'error',
          summary: 'Server Error',
          detail: 'An unexpected error occurred. Please try again later.',
        });
      } else if (error.status === 400) {
        const detail = error.error?.description || error.error?.title || 'Invalid request.';
        messageService.add({
          severity: 'warn',
          summary: 'Validation Error',
          detail,
        });
      }

      return throwError(() => error);
    }),
  );
};
