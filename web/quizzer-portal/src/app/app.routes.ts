import { Routes } from '@angular/router';
import { LayoutComponent } from './shared/layout/layout.component';
import { AuthLayoutComponent } from './shared/layout/auth-layout.component';
import { authGuard, guestGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { UserRole } from './core/models';

export const routes: Routes = [
  {
    path: 'auth',
    component: AuthLayoutComponent,
    canActivate: [guestGuard],
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent),
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/register/register.component').then((m) => m.RegisterComponent),
      },
      { path: '', redirectTo: 'login', pathMatch: 'full' },
    ],
  },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/profile/profile.component').then((m) => m.ProfileComponent),
      },
      {
        path: 'change-password',
        loadComponent: () => import('./features/profile/change-password.component').then((m) => m.ChangePasswordComponent),
      },
      {
        path: 'users',
        canActivate: [roleGuard],
        data: { allowedRoles: [UserRole.SuperAdmin, UserRole.SupportAdmin] },
        loadComponent: () => import('./features/user-management/user-list.component').then((m) => m.UserListComponent),
      },
      {
        path: 'tags',
        canActivate: [roleGuard],
        data: { allowedRoles: [UserRole.SuperAdmin, UserRole.SupportAdmin, UserRole.QuizAuthor] },
        loadComponent: () => import('./features/question-management/tag-list.component').then((m) => m.TagListComponent),
      },
      {
        path: 'question-sets',
        canActivate: [roleGuard],
        data: { allowedRoles: [UserRole.SuperAdmin, UserRole.SupportAdmin, UserRole.QuizAuthor] },
        loadComponent: () => import('./features/question-management/question-set-list.component').then((m) => m.QuestionSetListComponent),
      },
      {
        path: 'question-sets/:setId',
        canActivate: [roleGuard],
        data: { allowedRoles: [UserRole.SuperAdmin, UserRole.SupportAdmin, UserRole.QuizAuthor] },
        loadComponent: () => import('./features/question-management/question-set-detail.component').then((m) => m.QuestionSetDetailComponent),
      },
      {
        path: 'questions',
        canActivate: [roleGuard],
        data: { allowedRoles: [UserRole.SuperAdmin, UserRole.SupportAdmin, UserRole.QuizAuthor] },
        loadComponent: () => import('./features/question-management/question-list.component').then((m) => m.QuestionListComponent),
      },
      {
        path: 'exams',
        canActivate: [roleGuard],
        data: { allowedRoles: [UserRole.SuperAdmin, UserRole.SupportAdmin, UserRole.QuizAuthor] },
        loadComponent: () => import('./features/exam-management/exam-list.component').then((m) => m.ExamListComponent),
      },
      {
        path: 'available-exams',
        canActivate: [roleGuard],
        data: { allowedRoles: [UserRole.Examine] },
        loadComponent: () => import('./features/exam-taking/available-exams.component').then((m) => m.AvailableExamsComponent),
      },
      {
        path: 'exam/:examId/take',
        canActivate: [roleGuard],
        data: { allowedRoles: [UserRole.Examine] },
        loadComponent: () => import('./features/exam-taking/exam-taking.component').then((m) => m.ExamTakingComponent),
      },
      {
        path: 'my-results',
        canActivate: [roleGuard],
        data: { allowedRoles: [UserRole.Examine] },
        loadComponent: () => import('./features/exam-results/my-results.component').then((m) => m.MyResultsComponent),
      },
      {
        path: 'exam/:examId/result',
        canActivate: [roleGuard],
        data: { allowedRoles: [UserRole.Examine] },
        loadComponent: () => import('./features/exam-results/exam-result-detail.component').then((m) => m.ExamResultDetailComponent),
      },
      {
        path: 'exam/:examId/results',
        canActivate: [roleGuard],
        data: { allowedRoles: [UserRole.SuperAdmin, UserRole.SupportAdmin, UserRole.QuizAuthor] },
        loadComponent: () => import('./features/exam-results/exam-results-admin.component').then((m) => m.ExamResultsAdminComponent),
      },
      // AI routes - available to all authenticated users
      {
        path: 'ai/settings',
        loadComponent: () => import('./features/ai-settings/ai-settings.component').then((m) => m.AISettingsComponent),
      },
      {
        path: 'ai/generate',
        loadComponent: () => import('./features/ai-generation/generate-wizard/generate-wizard.component').then((m) => m.GenerateWizardComponent),
      },
      {
        path: 'ai/job-description',
        loadComponent: () => import('./features/ai-generation/job-description/job-description.component').then((m) => m.JobDescriptionComponent),
      },
      {
        path: 'ai/history',
        canActivate: [authGuard],
        loadComponent: () => import('./features/ai-generation/generation-history/generation-history.component').then((m) => m.GenerationHistoryComponent),
      },
      {
        path: 'ai/interview-prep',
        canActivate: [authGuard],
        loadComponent: () => import('./features/ai-generation/interview-prep-list/interview-prep-list.component').then((m) => m.InterviewPrepListComponent),
      },
      {
        path: 'ai/interview-prep/:id',
        canActivate: [authGuard],
        loadComponent: () => import('./features/ai-generation/interview-prep-detail/interview-prep-detail.component').then((m) => m.InterviewPrepDetailComponent),
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },
  {
    path: '**',
    loadComponent: () => import('./features/not-found/not-found.component').then((m) => m.NotFoundComponent),
  },
];
