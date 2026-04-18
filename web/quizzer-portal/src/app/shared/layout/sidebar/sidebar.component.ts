import { Component, ChangeDetectionStrategy, computed, inject, input, output } from '@angular/core';
import { NgClass } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { UserRole } from '../../../core/models';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [NgClass, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
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
    { label: 'AI Settings', icon: 'pi pi-cog', routerLink: ['/ai/settings'], visible: true },
    { label: 'Generate Quiz', icon: 'pi pi-sparkles', routerLink: ['/ai/generate'], visible: true },
    { label: 'Job Description', icon: 'pi pi-briefcase', routerLink: ['/ai/job-description'], visible: true },
    { label: 'Generation History', icon: 'pi pi-history', routerLink: ['/ai/history'], visible: true },
    { label: 'Interview Prep', icon: 'pi pi-graduation-cap', routerLink: ['/ai/interview-prep'], visible: true },
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
