import { Component, ChangeDetectionStrategy, inject, output, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { Button } from 'primeng/button';
import { Menu } from 'primeng/menu';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [Button, Menu],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="topbar">
      <div class="topbar-left">
        <p-button
          icon="pi pi-bars"
          [text]="true"
          [rounded]="true"
          severity="secondary"
          (onClick)="toggleSidebar.emit()"
        />
      </div>
      <div class="topbar-right">
        <p-button
          [icon]="isDarkMode() ? 'pi pi-sun' : 'pi pi-moon'"
          [text]="true"
          [rounded]="true"
          severity="secondary"
          (onClick)="toggleDarkMode()"
        />
        <p-button
          [label]="userName()"
          icon="pi pi-user"
          [text]="true"
          severity="secondary"
          (onClick)="userMenu.toggle($event)"
        />
        <p-menu #userMenu [model]="userMenuItems" [popup]="true" />
      </div>
    </div>
  `,
  styles: [`
    .topbar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0 1rem;
      height: 64px;
      background: var(--p-surface-0);
      border-bottom: 1px solid var(--p-surface-200);
    }

    .topbar-left {
      display: flex;
      align-items: center;
    }

    .topbar-right {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
  `],
})
export class TopbarComponent {
  toggleSidebar = output();
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isDarkMode = signal(localStorage.getItem('darkMode') === 'true');

  readonly userName = signal('');

  readonly userMenuItems: MenuItem[] = [
    { label: 'Profile', icon: 'pi pi-user', command: () => this.router.navigate(['/profile']) },
    { label: 'Change Password', icon: 'pi pi-key', command: () => this.router.navigate(['/change-password']) },
    { separator: true },
    { label: 'Logout', icon: 'pi pi-sign-out', command: () => this.authService.logout() },
  ];

  constructor() {
    // Apply dark mode on init
    if (this.isDarkMode()) {
      document.documentElement.classList.add('dark-mode');
    }
    // Update username reactively
    const user = this.authService.currentUser();
    if (user) {
      this.userName.set(`${user.firstName} ${user.lastName}`);
    }
  }

  toggleDarkMode(): void {
    const newValue = !this.isDarkMode();
    this.isDarkMode.set(newValue);
    localStorage.setItem('darkMode', String(newValue));
    if (newValue) {
      document.documentElement.classList.add('dark-mode');
    } else {
      document.documentElement.classList.remove('dark-mode');
    }
  }
}
