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
  templateUrl: './available-exams.component.html',
  styleUrl: './available-exams.component.scss',
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
