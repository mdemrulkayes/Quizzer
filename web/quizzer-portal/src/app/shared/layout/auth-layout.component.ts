import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Toast } from 'primeng/toast';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [RouterOutlet, Toast],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="auth-layout">
      <div class="auth-container">
        <div class="auth-header">
          <i class="pi pi-book auth-logo"></i>
          <h1>Quizzer</h1>
        </div>
        <router-outlet />
      </div>
    </div>
    <p-toast />
  `,
  styles: [`
    .auth-layout {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--p-surface-50);
      padding: 1rem;
    }

    .auth-container {
      width: 100%;
      max-width: 440px;
    }

    .auth-header {
      text-align: center;
      margin-bottom: 2rem;
    }

    .auth-logo {
      font-size: 3rem;
      color: var(--p-primary-color);
    }

    .auth-header h1 {
      margin: 0.5rem 0 0;
      font-size: 1.75rem;
      font-weight: 700;
      color: var(--p-text-color);
    }
  `],
})
export class AuthLayoutComponent {}
