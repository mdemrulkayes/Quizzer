import { Component, ChangeDetectionStrategy, inject, computed } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';
import { UserRole } from '../../core/models';
import { Card } from 'primeng/card';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [Card],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h2>Dashboard</h2>
    <p>Welcome, {{ authService.currentUser()?.firstName ?? 'User' }}!</p>

    <div class="dashboard-grid">
      @switch (dashboardType()) {
        @case ('admin') {
          <p-card header="Users" subheader="Manage all users">
            <p>View and manage user accounts and roles.</p>
          </p-card>
          <p-card header="Exams" subheader="Manage exams">
            <p>Create, publish, and monitor exams.</p>
          </p-card>
          <p-card header="Questions" subheader="Question bank">
            <p>Manage question sets and tags.</p>
          </p-card>
        }
        @case ('author') {
          <p-card header="My Question Sets" subheader="Create & manage">
            <p>Build and organize question sets for exams.</p>
          </p-card>
          <p-card header="My Exams" subheader="Create & publish">
            <p>Create exams and publish them for students.</p>
          </p-card>
        }
        @case ('examinee') {
          <p-card header="Available Exams" subheader="Take exams">
            <p>Browse and take published exams.</p>
          </p-card>
          <p-card header="My Results" subheader="View results">
            <p>Check your exam scores and history.</p>
          </p-card>
        }
      }
    </div>
  `,
  styles: [`
    h2 {
      margin: 0 0 0.5rem;
      color: var(--p-text-color);
    }

    p {
      color: var(--p-text-muted-color);
      margin-bottom: 1.5rem;
    }

    .dashboard-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 1.5rem;
    }
  `],
})
export class DashboardComponent {
  readonly authService = inject(AuthService);

  readonly dashboardType = computed(() => {
    const roles = this.authService.userRoles();
    if (roles.includes(UserRole.SuperAdmin) || roles.includes(UserRole.SupportAdmin)) {
      return 'admin';
    }
    if (roles.includes(UserRole.QuizAuthor)) {
      return 'author';
    }
    return 'examinee';
  });
}
