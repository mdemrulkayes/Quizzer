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
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.scss',
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
