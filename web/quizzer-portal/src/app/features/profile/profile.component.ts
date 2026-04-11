import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';
import { Card } from 'primeng/card';
import { Tag } from 'primeng/tag';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [Card, Tag],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h2>My Profile</h2>
    @if (authService.currentUser(); as user) {
      <p-card>
        <div class="profile-info">
          <div class="field"><label>Name</label><span>{{ user.firstName }} {{ user.lastName }}</span></div>
          <div class="field"><label>Email</label><span>{{ user.email }}</span></div>
          <div class="field">
            <label>Roles</label>
            <div class="roles">
              @for (role of user.roles; track role) {
                <p-tag [value]="role" severity="info" />
              }
            </div>
          </div>
        </div>
      </p-card>
    }
  `,
  styles: [`
    h2 { margin: 0 0 1rem; color: var(--p-text-color); }
    .profile-info { display: flex; flex-direction: column; gap: 1rem; }
    .field { display: flex; flex-direction: column; gap: 0.25rem; }
    .field label { font-weight: 600; font-size: 0.875rem; color: var(--p-text-muted-color); }
    .roles { display: flex; gap: 0.5rem; flex-wrap: wrap; }
  `],
})
export class ProfileComponent {
  readonly authService = inject(AuthService);
}
