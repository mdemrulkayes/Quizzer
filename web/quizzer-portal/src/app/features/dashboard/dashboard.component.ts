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
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
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
