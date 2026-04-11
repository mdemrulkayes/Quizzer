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
  template: `
    <h2>My Exam Results</h2>

    <p-table
      [value]="results()"
      [lazy]="true"
      [paginator]="true"
      [rows]="10"
      [totalRecords]="totalRecords()"
      [loading]="loading()"
      [rowsPerPageOptions]="[5, 10, 20]"
      [rowHover]="true"
      (onLazyLoad)="onLazyLoad($event)"
    >
      <ng-template #header>
        <tr>
          <th>Exam Title</th>
          <th>Started At</th>
          <th>Submitted At</th>
          <th>Status</th>
          <th>Score</th>
          <th>Pass/Fail</th>
        </tr>
      </ng-template>

      <ng-template #body let-exam>
        <tr (click)="viewResult(exam)" style="cursor: pointer;">
          <td>{{ exam.examTitle }}</td>
          <td>{{ exam.startedAt | date: 'medium' }}</td>
          <td>{{ exam.submittedAt ? (exam.submittedAt | date: 'medium') : '-' }}</td>
          <td>
            <p-tag [value]="exam.status" [severity]="getStatusSeverity(exam.status)" />
          </td>
          <td>{{ exam.totalScore !== null ? exam.totalScore : '-' }}</td>
          <td>
            @if (exam.isPassed === true) {
              <p-tag value="Passed" severity="success" />
            } @else if (exam.isPassed === false) {
              <p-tag value="Failed" severity="danger" />
            } @else {
              -
            }
          </td>
        </tr>
      </ng-template>

      <ng-template #emptymessage>
        <tr>
          <td colspan="6" style="text-align: center;">No results found.</td>
        </tr>
      </ng-template>
    </p-table>
  `,
  styles: `
    :host {
      display: block;
      padding: 1rem;
    }
  `,
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
