import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { UserType } from '../../../core/models';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Password } from 'primeng/password';
import { Button } from 'primeng/button';
import { FloatLabel } from 'primeng/floatlabel';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, Card, InputText, Password, Button, FloatLabel],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <p-card>
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <div class="form-fields">
          <div class="form-row">
            <p-floatlabel variant="on">
              <input pInputText id="firstName" formControlName="firstName" class="w-full" />
              <label for="firstName">First Name</label>
            </p-floatlabel>
            <p-floatlabel variant="on">
              <input pInputText id="lastName" formControlName="lastName" class="w-full" />
              <label for="lastName">Last Name</label>
            </p-floatlabel>
          </div>
          <p-floatlabel variant="on">
            <input pInputText id="email" formControlName="email" class="w-full" />
            <label for="email">Email</label>
          </p-floatlabel>
          <p-floatlabel variant="on">
            <input pInputText id="phoneNumber" formControlName="phoneNumber" class="w-full" />
            <label for="phoneNumber">Phone Number</label>
          </p-floatlabel>
          <p-floatlabel variant="on">
            <p-password id="password" formControlName="password" [toggleMask]="true" styleClass="w-full" inputStyleClass="w-full" />
            <label for="password">Password</label>
          </p-floatlabel>
          <p-floatlabel variant="on">
            <p-password id="confirmPassword" formControlName="confirmPassword" [feedback]="false" [toggleMask]="true" styleClass="w-full" inputStyleClass="w-full" />
            <label for="confirmPassword">Confirm Password</label>
          </p-floatlabel>
          @if (form.hasError('passwordMismatch')) {
            <small class="p-error">Passwords do not match.</small>
          }
          @if (errorMessage()) {
            <small class="p-error">{{ errorMessage() }}</small>
          }
          <p-button
            type="submit"
            label="Create Account"
            icon="pi pi-user-plus"
            [loading]="loading()"
            [disabled]="form.invalid"
            styleClass="w-full"
          />
        </div>
        <div class="form-footer">
          <span>Already have an account? <a routerLink="/auth/login">Sign In</a></span>
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

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
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
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);

  readonly loading = signal(false);
  readonly errorMessage = signal('');

  readonly form = this.fb.nonNullable.group(
    {
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: [this.passwordMatchValidator] },
  );

  private passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.errorMessage.set('');

    const formValue = this.form.getRawValue();
    this.authService
      .register({
        ...formValue,
        userType: UserType.Examine,
      })
      .subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Account created! Please sign in.' });
          this.loading.set(false);
          this.router.navigate(['/auth/login']);
        },
        error: (err) => {
          this.loading.set(false);
          this.errorMessage.set(err.error?.description || 'Registration failed. Please try again.');
        },
      });
  }
}
