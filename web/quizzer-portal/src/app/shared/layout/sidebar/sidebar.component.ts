import { Component, ChangeDetectionStrategy, computed, inject, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { UserRole } from '../../../core/models';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="sidebar" [class.collapsed]="collapsed()">
      <div class="sidebar-header">
        <span class="logo" [class.hidden]="collapsed()">
          <i class="pi pi-book"></i>
          @if (!collapsed()) {
            <span class="logo-text">Quizzer</span>
          }
        </span>
      </div>
      <nav class="sidebar-nav">
        @for (item of filteredMenuItems(); track item.label) {
          <a
            class="nav-item"
            [routerLink]="item.routerLink"
            routerLinkActive="active"
            [routerLinkActiveOptions]="{ exact: item.routerLink?.[0] === '/dashboard' }"
            [title]="item.label || ''"
          >
            <i [class]="item.icon || ''"></i>
            @if (!collapsed()) {
              <span>{{ item.label }}</span>
            }
          </a>
        }
      </nav>
    </div>
  `,
  styles: [`
    .sidebar {
      width: 260px;
      min-height: 100vh;
      background: var(--p-surface-0);
      border-right: 1px solid var(--p-surface-200);
      display: flex;
      flex-direction: column;
      transition: width 0.2s ease;
      overflow: hidden;
    }

    .sidebar.collapsed {
      width: 64px;
    }

    .sidebar-header {
      padding: 1rem;
      display: flex;
      align-items: center;
      justify-content: center;
      border-bottom: 1px solid var(--p-surface-200);
      min-height: 64px;
    }

    .logo {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 1.25rem;
      font-weight: 700;
      color: var(--p-primary-color);
    }

    .logo.hidden .logo-text {
      display: none;
    }

    .logo i {
      font-size: 1.5rem;
    }

    .sidebar-nav {
      display: flex;
      flex-direction: column;
      padding: 0.5rem;
      gap: 0.25rem;
      flex: 1;
    }

    .nav-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.75rem 1rem;
      border-radius: var(--p-border-radius);
      color: var(--p-text-color);
      text-decoration: none;
      transition: background 0.15s ease;
      white-space: nowrap;
    }

    .nav-item:hover {
      background: var(--p-surface-100);
    }

    .nav-item.active {
      background: var(--p-primary-color);
      color: var(--p-primary-contrast-color);
    }

    .nav-item i {
      font-size: 1.1rem;
      width: 1.25rem;
      text-align: center;
    }
  `],
})
export class SidebarComponent {
  collapsed = input(false);
  private readonly authService = inject(AuthService);

  private readonly allMenuItems: MenuItem[] = [
    { label: 'Dashboard', icon: 'pi pi-home', routerLink: ['/dashboard'], visible: true },
    { label: 'Users', icon: 'pi pi-users', routerLink: ['/users'], visible: false, id: 'admin' },
    { label: 'Question Sets', icon: 'pi pi-list', routerLink: ['/question-sets'], visible: false, id: 'author' },
    { label: 'Questions', icon: 'pi pi-question-circle', routerLink: ['/questions'], visible: false, id: 'author' },
    { label: 'Tags', icon: 'pi pi-tags', routerLink: ['/tags'], visible: false, id: 'author' },
    { label: 'Exams', icon: 'pi pi-file-edit', routerLink: ['/exams'], visible: false, id: 'author' },
    { label: 'Available Exams', icon: 'pi pi-play-circle', routerLink: ['/available-exams'], visible: false, id: 'examinee' },
    { label: 'My Results', icon: 'pi pi-chart-bar', routerLink: ['/my-results'], visible: false, id: 'examinee' },
  ];

  readonly filteredMenuItems = computed(() => {
    const roles = this.authService.userRoles();
    return this.allMenuItems.filter((item) => {
      if (item.visible) return true;
      if (item.id === 'admin') {
        return roles.includes(UserRole.SuperAdmin) || roles.includes(UserRole.SupportAdmin);
      }
      if (item.id === 'author') {
        return roles.includes(UserRole.SuperAdmin) || roles.includes(UserRole.SupportAdmin) || roles.includes(UserRole.QuizAuthor);
      }
      if (item.id === 'examinee') {
        return roles.includes(UserRole.Examine);
      }
      return false;
    });
  });
}
