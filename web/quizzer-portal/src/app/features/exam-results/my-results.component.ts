import { Component, ChangeDetectionStrategy, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { MessageService } from 'primeng/api';
import { ExamService } from '../../core/services/exam.service';
import { ExamAttemptResponse, ExamAttemptStatus } from '../../core/models';

@Component({
  selector: 'app-my-results',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [TableModule, Tag, DatePipe],
  templateUrl: './my-results.component.html',
  styleUrl: './my-results.component.scss',
})
export class MyResultsComponent implements OnInit {
  private readonly examService = inject(ExamService);
  private readonly messageService = inject(MessageService);
  private readonly router = inject(Router);

  readonly results = signal<ExamAttemptResponse[]>([]);
  readonly loading = signal(false);
  readonly totalRecords = signal(0);

  ngOnInit(): void {
    this.loadResults(1, 10);
  }

  onLazyLoad(event: TableLazyLoadEvent): void {
    const rows = event.rows ?? 10;
    const pageNumber = Math.floor((event.first ?? 0) / rows) + 1;
    this.loadResults(pageNumber, rows);
  }

  viewResult(exam: ExamAttemptResponse): void {
    this.router.navigate(['/exam', exam.examId, 'result']);
  }

  getStatusSeverity(status: ExamAttemptStatus): 'info' | 'warn' | 'danger' | 'success' | 'secondary' {
    switch (status) {
      case ExamAttemptStatus.InProgress:
        return 'info';
      case ExamAttemptStatus.Submitted:
        return 'warn';
      case ExamAttemptStatus.TimedOut:
        return 'danger';
      case ExamAttemptStatus.Graded:
        return 'success';
      case ExamAttemptStatus.Cancelled:
        return 'secondary';
      default:
        return 'info';
    }
  }

  private loadResults(pageNumber: number, pageSize: number): void {
    this.loading.set(true);
    this.examService.getMyAllResults(pageNumber, pageSize).subscribe({
      next: (response) => {
        this.results.set(response.items);
        this.totalRecords.set(response.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load exam results.',
        });
        this.loading.set(false);
      },
    });
  }
}
