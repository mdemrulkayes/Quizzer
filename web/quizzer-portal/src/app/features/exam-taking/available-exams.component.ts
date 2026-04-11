import { Component, ChangeDetectionStrategy, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { Card } from 'primeng/card';
import { Button } from 'primeng/button';
import { Tag } from 'primeng/tag';
import { Dialog } from 'primeng/dialog';
import { ProgressSpinner } from 'primeng/progressspinner';
import { MessageService } from 'primeng/api';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { ExamService } from '../../core/services/exam.service';
import { ExamResponse } from '../../core/models';

@Component({
  selector: 'app-available-exams',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Card, Button, Tag, Dialog, ProgressSpinner, Paginator],
  styles: `
    :host {
      display: block;
      padding: 2rem;
    }

    .page-header {
      margin-bottom: 2rem;
    }

    .exams-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
      gap: 1.5rem;
    }

    .exam-badges {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      margin: 1rem 0;
    }

    .exam-description {
      color: var(--text-color-secondary);
      margin-bottom: 1rem;
      min-height: 2.5rem;
    }

    .exam-actions {
      display: flex;
      justify-content: flex-end;
    }

    .loading-container,
    .empty-container {
      display: flex;
      justify-content: center;
      align-items: center;
      padding: 4rem 0;
    }

    .empty-container {
      flex-direction: column;
      gap: 1rem;
      color: var(--text-color-secondary);
    }

    .empty-container i {
      font-size: 3rem;
    }

    .confirm-details {
      margin: 1rem 0;
    }

    .confirm-details li {
      margin-bottom: 0.5rem;
    }

    .confirm-warning {
      margin-top: 1rem;
      font-weight: 600;
      color: var(--orange-500);
    }

    .dialog-footer {
      display: flex;
      justify-content: flex-end;
      gap: 0.5rem;
      margin-top: 1.5rem;
    }

    .paginator-container {
      margin-top: 2rem;
    }
  `,
  template: `
    <div class="page-header">
      <h2>Available Exams</h2>
    </div>

    @if (loading()) {
      <div class="loading-container">
        <p-progressspinner strokeWidth="4" />
      </div>
    } @else if (availableExams().length === 0) {
      <div class="empty-container">
        <i class="pi pi-inbox"></i>
        <p>No exams available</p>
      </div>
    } @else {
      <div class="exams-grid">
        @for (exam of availableExams(); track exam.examId) {
          <p-card [header]="exam.title">
            <p class="exam-description">
              {{ exam.description ?? 'No description provided.' }}
            </p>
            <div class="exam-badges">
              <p-tag severity="info" [value]="exam.durationInMinutes + ' min'" icon="pi pi-clock" />
              <p-tag severity="success" [value]="'Total: ' + exam.totalMarks" icon="pi pi-star" />
              <p-tag severity="warn" [value]="'Pass: ' + exam.passingMarks" icon="pi pi-check-circle" />
            </div>
            <div class="exam-actions">
              <p-button label="Start Exam" icon="pi pi-play" (onClick)="onStartClick(exam)" />
            </div>
          </p-card>
        }
      </div>

      <div class="paginator-container">
        <p-paginator
          [rows]="pageSize()"
          [totalRecords]="totalRecords()"
          [first]="first()"
          (onPageChange)="onPageChange($event)"
        />
      </div>
    }

    <p-dialog
      header="Confirm Start Exam"
      [visible]="dialogVisible()"
      [modal]="true"
      [closable]="true"
      [style]="{ width: '450px' }"
      (visibleChange)="onDialogVisibleChange($event)"
    >
      @if (selectedExam(); as exam) {
        <p>Are you sure you want to start <strong>{{ exam.title }}</strong>?</p>
        <ul class="confirm-details">
          <li><strong>Duration:</strong> {{ exam.durationInMinutes }} minutes</li>
          <li><strong>Total Marks:</strong> {{ exam.totalMarks }}</li>
          <li><strong>Passing Marks:</strong> {{ exam.passingMarks }}</li>
        </ul>
        <p class="confirm-warning">
          Once started, you will have {{ exam.durationInMinutes }} minutes to complete it.
        </p>
        <div class="dialog-footer">
          <p-button label="Cancel" severity="secondary" (onClick)="onDialogCancel()" [disabled]="starting()" />
          <p-button label="Start" icon="pi pi-play" (onClick)="onDialogConfirm()" [loading]="starting()" />
        </div>
      }
    </p-dialog>
  `,
})
export class AvailableExamsComponent {
  private readonly examService = inject(ExamService);
  private readonly messageService = inject(MessageService);
  private readonly router = inject(Router);

  readonly availableExams = signal<ExamResponse[]>([]);
  readonly loading = signal(true);
  readonly totalRecords = signal(0);
  readonly page = signal(1);
  readonly pageSize = signal(10);
  readonly first = computed(() => (this.page() - 1) * this.pageSize());

  readonly selectedExam = signal<ExamResponse | null>(null);
  readonly dialogVisible = signal(false);
  readonly starting = signal(false);

  constructor() {
    this.loadExams();
  }

  loadExams(): void {
    this.loading.set(true);
    this.examService.getAvailableExams(this.page(), this.pageSize()).subscribe({
      next: (result) => {
        this.availableExams.set(result.items);
        this.totalRecords.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load available exams' });
        this.loading.set(false);
      },
    });
  }

  onPageChange(event: PaginatorState): void {
    const newPage = (event.page ?? 0) + 1;
    this.page.set(newPage);
    this.loadExams();
  }

  onStartClick(exam: ExamResponse): void {
    this.selectedExam.set(exam);
    this.dialogVisible.set(true);
  }

  onDialogCancel(): void {
    this.dialogVisible.set(false);
    this.selectedExam.set(null);
  }

  onDialogVisibleChange(visible: boolean): void {
    if (!visible) {
      this.onDialogCancel();
    }
  }

  onDialogConfirm(): void {
    const exam = this.selectedExam();
    if (!exam) return;

    this.starting.set(true);
    this.examService.startExam(exam.examId).subscribe({
      next: (response) => {
        this.starting.set(false);
        this.dialogVisible.set(false);
        this.selectedExam.set(null);
        this.router.navigate(['/exam', exam.examId, 'take'], { state: { attempt: response } });
      },
      error: () => {
        this.starting.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to start exam' });
      },
    });
  }
}
