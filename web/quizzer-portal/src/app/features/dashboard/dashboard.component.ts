import { Component, ChangeDetectionStrategy, inject, computed, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { UserRole } from '../../core/models';
import { IdentityService } from '../../core/services/identity.service';
import { ExamService } from '../../core/services/exam.service';
import { QuizService } from '../../core/services/quiz.service';
import { TitleCasePipe } from '@angular/common';
import { Card } from 'primeng/card';
import { Tag } from 'primeng/tag';

interface DashboardCard {
  icon: string;
  color: string;
  count: number;
  label: string;
  route: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [TitleCasePipe, Card, Tag],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="dashboard-header">
      <h2>Dashboard</h2>
      <p class="welcome-text">
        Welcome back, <strong>{{ authService.currentUser()?.firstName ?? 'User' }}</strong>!
        <p-tag [value]="dashboardType() | titlecase" severity="info" />
      </p>
    </div>

    @if (loading()) {
      <div class="dashboard-grid">
        @for (i of [1, 2, 3]; track i) {
          <p-card styleClass="dashboard-card">
            <div class="card-content">
              <div class="card-icon skeleton-icon"></div>
              <div class="card-info">
                <span class="card-count skeleton-text">&nbsp;</span>
                <span class="card-label skeleton-text">&nbsp;</span>
              </div>
            </div>
          </p-card>
        }
      </div>
    } @else {
      <div class="dashboard-grid">
        @for (card of cards(); track card.label) {
          <p-card styleClass="dashboard-card" (click)="navigateTo(card.route)">
            <div class="card-content">
              <div class="card-icon">
                <i [class]="card.icon" [style.color]="card.color"></i>
              </div>
              <div class="card-info">
                <span class="card-count">{{ card.count }}</span>
                <span class="card-label">{{ card.label }}</span>
              </div>
            </div>
          </p-card>
        }
      </div>
    }
  `,
  styles: [`
    .dashboard-header {
      margin-bottom: 2rem;
    }

    .dashboard-header h2 {
      margin: 0 0 0.5rem;
      color: var(--p-text-color);
      font-size: 1.75rem;
    }

    .welcome-text {
      color: var(--p-text-muted-color);
      margin: 0;
      display: flex;
      align-items: center;
      gap: 0.75rem;
      font-size: 1.05rem;
    }

    .dashboard-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
      gap: 1.5rem;
    }

    :host ::ng-deep .dashboard-card {
      cursor: pointer;
      transition: transform 0.2s ease, box-shadow 0.2s ease;
    }

    :host ::ng-deep .dashboard-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
    }

    .card-content {
      display: flex;
      align-items: center;
      gap: 1.25rem;
      padding: 0.5rem 0;
    }

    .card-icon {
      width: 56px;
      height: 56px;
      border-radius: 12px;
      background: var(--p-surface-100);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .card-icon i {
      font-size: 1.75rem;
    }

    .card-info {
      display: flex;
      flex-direction: column;
    }

    .card-count {
      font-size: 2rem;
      font-weight: 700;
      line-height: 1.2;
      color: var(--p-text-color);
    }

    .card-label {
      font-size: 0.9rem;
      color: var(--p-text-muted-color);
      margin-top: 0.15rem;
    }

    .skeleton-icon {
      background: linear-gradient(90deg, var(--p-surface-100) 25%, var(--p-surface-200) 50%, var(--p-surface-100) 75%);
      background-size: 200% 100%;
      animation: shimmer 1.5s infinite;
    }

    .skeleton-text {
      background: linear-gradient(90deg, var(--p-surface-100) 25%, var(--p-surface-200) 50%, var(--p-surface-100) 75%);
      background-size: 200% 100%;
      animation: shimmer 1.5s infinite;
      border-radius: 4px;
      min-width: 80px;
      display: inline-block;
    }

    @keyframes shimmer {
      0% { background-position: -200% 0; }
      100% { background-position: 200% 0; }
    }
  `],
})
export class DashboardComponent implements OnInit {
  readonly authService = inject(AuthService);
  private readonly identityService = inject(IdentityService);
  private readonly examService = inject(ExamService);
  private readonly quizService = inject(QuizService);
  private readonly router = inject(Router);

  readonly loading = signal(true);
  readonly cards = signal<DashboardCard[]>([]);

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

  ngOnInit(): void {
    this.loadDashboardData();
  }

  navigateTo(route: string): void {
    this.router.navigate([route]);
  }

  private loadDashboardData(): void {
    const type = this.dashboardType();

    switch (type) {
      case 'admin':
        this.loadAdminData();
        break;
      case 'author':
        this.loadAuthorData();
        break;
      case 'examinee':
        this.loadExamineeData();
        break;
    }
  }

  private loadAdminData(): void {
    forkJoin({
      users: this.identityService.getUsers(1, 1),
      exams: this.examService.getExams(1, 1),
      questionSets: this.quizService.getQuestionSets({ pageNumber: 1, pageSize: 1 }),
    }).subscribe({
      next: ({ users, exams, questionSets }) => {
        this.cards.set([
          { icon: 'pi pi-users', color: '#6366f1', count: users.totalCount, label: 'Total Users', route: '/users' },
          { icon: 'pi pi-file-edit', color: '#f59e0b', count: exams.totalCount, label: 'Total Exams', route: '/exams' },
          { icon: 'pi pi-list', color: '#10b981', count: questionSets.totalCount, label: 'Question Sets', route: '/question-sets' },
        ]);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }

  private loadAuthorData(): void {
    forkJoin({
      questionSets: this.quizService.getQuestionSets({ pageNumber: 1, pageSize: 1 }),
      exams: this.examService.getExams(1, 1),
    }).subscribe({
      next: ({ questionSets, exams }) => {
        this.cards.set([
          { icon: 'pi pi-list', color: '#10b981', count: questionSets.totalCount, label: 'Question Sets', route: '/question-sets' },
          { icon: 'pi pi-file-edit', color: '#f59e0b', count: exams.totalCount, label: 'My Exams', route: '/exams' },
        ]);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }

  private loadExamineeData(): void {
    forkJoin({
      available: this.examService.getAvailableExams(1, 1),
      results: this.examService.getMyAllResults(1, 1),
    }).subscribe({
      next: ({ available, results }) => {
        this.cards.set([
          { icon: 'pi pi-book', color: '#6366f1', count: available.totalCount, label: 'Available Exams', route: '/available-exams' },
          { icon: 'pi pi-chart-bar', color: '#10b981', count: results.totalCount, label: 'My Results', route: '/my-results' },
        ]);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }
}
