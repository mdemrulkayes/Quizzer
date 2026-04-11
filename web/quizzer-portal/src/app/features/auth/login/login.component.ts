import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { MessageService } from 'primeng/api';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Password } from 'primeng/password';
import { Button } from 'primeng/button';
import { FloatLabel } from 'primeng/floatlabel';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, Card, InputText, Password, Button, FloatLabel],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <p-card>
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <div class="form-fields">
          <p-floatlabel variant="on">
            <input pInputText id="email" formControlName="email" class="w-full" />
            <label for="email">Email</label>
          </p-floatlabel>
          <p-floatlabel variant="on">
            <p-password id="password" formControlName="password" [feedback]="false" [toggleMask]="true" styleClass="w-full" inputStyleClass="w-full" />
            <label for="password">Password</label>
          </p-floatlabel>
          @if (errorMessage()) {
            <small class="p-error">{{ errorMessage() }}</small>
          }
          <p-button
            type="submit"
            label="Sign In"
            icon="pi pi-sign-in"
            [loading]="loading()"
            [disabled]="form.invalid"
            styleClass="w-full"
          />
        </div>
        <div class="form-footer">
          <span>Don't have an account? <a routerLink="/auth/register">Register</a></span>
        </div>
      </form>
    </p-card>
  `,
  styles: [`
    .form-fields {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .form-footer {
      text-align: center;
      margin-top: 1.5rem;
      font-size: 0.875rem;
    }

    .form-footer a {
      color: var(--p-primary-color);
      text-decoration: none;
      font-weight: 600;
    }

    :host ::ng-deep .p-card-body {
      padding: 2rem;
    }
  `],
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);

  readonly loading = signal(false);
  readonly errorMessage = signal('');

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.errorMessage.set('');

    const { email, password } = this.form.getRawValue();
    this.authService.login({ email, password }).subscribe({
      next: () => {
        this.authService.loadProfile().subscribe({
          next: () => {
            this.loading.set(false);
            this.router.navigate(['/dashboard']);
          },
          error: () => {
            this.loading.set(false);
            this.errorMessage.set('Failed to load profile.');
          },
        });
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.description || 'Invalid email or password.');
      },
    });
  }
}
