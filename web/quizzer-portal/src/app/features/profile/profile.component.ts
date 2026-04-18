import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { MessageService } from 'primeng/api';
import { Card } from 'primeng/card';
import { Tag } from 'primeng/tag';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [FormsModule, Card, Tag, Button, InputText],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class ProfileComponent {
  readonly authService = inject(AuthService);
  private readonly messageService = inject(MessageService);

  editMode = signal(false);
  saving = signal(false);
  editFirstName = '';
  editLastName = '';

  startEdit(): void {
    const user = this.authService.currentUser();
    if (!user) return;
    this.editFirstName = user.firstName;
    this.editLastName = user.lastName;
    this.editMode.set(true);
  }

  cancelEdit(): void {
    this.editMode.set(false);
  }

  saveProfile(): void {
    if (!this.editFirstName.trim() || !this.editLastName.trim()) return;
    this.saving.set(true);
    this.authService.updateProfile(this.editFirstName.trim(), this.editLastName.trim()).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Profile updated successfully.' });
        this.editMode.set(false);
        this.saving.set(false);
      },
      error: () => {
        this.saving.set(false);
      },
    });
  }
}
