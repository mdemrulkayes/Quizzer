import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { MessageService } from 'primeng/api';
import { Card } from 'primeng/card';
import { Password } from 'primeng/password';
import { Button } from 'primeng/button';
import { FloatLabel } from 'primeng/floatlabel';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [ReactiveFormsModule, Card, Password, Button, FloatLabel],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h2>Change Password</h2>
    <p-card styleClass="max-w-lg">
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <div class="form-fields">
          <p-floatlabel variant="on">
            <p-password id="currentPassword" formControlName="currentPassword" [feedback]="false" [toggleMask]="true" styleClass="w-full" inputStyleClass="w-full" />
            <label for="currentPassword">Current Password</label>
          </p-floatlabel>
          <p-floatlabel variant="on">
            <p-password id="newPassword" formControlName="newPassword" [toggleMask]="true" styleClass="w-full" inputStyleClass="w-full" />
            <label for="newPassword">New Password</label>
          </p-floatlabel>
          <p-floatlabel variant="on">
            <p-password id="confirmNewPassword" formControlName="confirmNewPassword" [feedback]="false" [toggleMask]="true" styleClass="w-full" inputStyleClass="w-full" />
            <label for="confirmNewPassword">Confirm New Password</label>
          </p-floatlabel>
          @if (form.hasError('passwordMismatch')) {
            <small class="p-error">New passwords do not match.</small>
          }
          <p-button type="submit" label="Change Password" icon="pi pi-key" [loading]="loading()" [disabled]="form.invalid" />
        </div>
      </form>
    </p-card>
  `,
  styles: [`
    h2 { margin: 0 0 1rem; color: var(--p-text-color); }
    .form-fields { display: flex; flex-direction: column; gap: 1.5rem; }
  `],
})
export class ChangePasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly messageService = inject(MessageService);

  readonly loading = signal(false);

  readonly form = this.fb.nonNullable.group(
    {
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmNewPassword: ['', [Validators.required]],
    },
    { validators: [this.passwordMatchValidator] },
  );

  private passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const newPassword = control.get('newPassword')?.value;
    const confirmNewPassword = control.get('confirmNewPassword')?.value;
    return newPassword === confirmNewPassword ? null : { passwordMismatch: true };
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    const { currentPassword, newPassword, confirmNewPassword } = this.form.getRawValue();
    this.authService.changePassword({ currentPassword, newPassword, confirmNewPassword }).subscribe({
      next: () => {
        this.loading.set(false);
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Password changed successfully.' });
        this.form.reset();
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }
}
