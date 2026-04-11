import { Component, ChangeDetectionStrategy, inject, signal, input, effect } from '@angular/core';
import { DatePipe } from '@angular/common';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { MessageService } from 'primeng/api';
import { ExamService } from '../../core/services/exam.service';
import { ExamAttemptResponse, ExamAttemptStatus } from '../../core/models';

@Component({
  selector: 'app-exam-results-admin',
  standalone: true,
  imports: [DatePipe, TableModule, Tag],
  providers: [MessageService],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './exam-results-admin.component.html',
  styleUrl: './exam-results-admin.component.scss',
})
export class ExamResultsAdminComponent {
  private readonly examService = inject(ExamService);
  private readonly messageService = inject(MessageService);

  readonly examId = input.required<string>();

  readonly results = signal<ExamAttemptResponse[]>([]);
  readonly totalRecords = signal(0);
  readonly loading = signal(true);
  readonly examTitle = signal('Exam Results');

  readonly pageSize = 10;

  constructor() {
    effect(() => {
      const id = this.examId();
      if (id) {
        this.loadExamDetails(+id);
        this.loadResults(+id, 1, this.pageSize);
      }
    });
  }

  onLazyLoad(event: TableLazyLoadEvent): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? this.pageSize;
    const pageNumber = Math.floor(first / rows) + 1;
    this.loadResults(+this.examId(), pageNumber, rows);
  }

  getStatusSeverity(status: ExamAttemptStatus): 'info' | 'warn' | 'danger' | 'success' | 'secondary' {
    switch (status) {
      case ExamAttemptStatus.InProgress: return 'info';
      case ExamAttemptStatus.Submitted: return 'warn';
      case ExamAttemptStatus.TimedOut: return 'danger';
      case ExamAttemptStatus.Graded: return 'success';
      case ExamAttemptStatus.Cancelled: return 'secondary';
      default: return 'secondary';
    }
  }

  private loadExamDetails(examId: number): void {
    this.examService.getExamById(examId).subscribe({
      next: (exam) => {
        this.examTitle.set(`Results — ${exam.title}`);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load exam details.' });
      },
    });
  }

  private loadResults(examId: number, pageNumber: number, pageSize: number): void {
    this.loading.set(true);
    this.examService.getExamResults(examId, pageNumber, pageSize).subscribe({
      next: (response) => {
        this.results.set(response.items);
        this.totalRecords.set(response.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load exam results.' });
      },
    });
  }
}
