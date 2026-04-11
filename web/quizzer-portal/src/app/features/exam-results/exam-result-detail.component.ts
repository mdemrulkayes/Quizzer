import { Component, ChangeDetectionStrategy, inject, input, signal, computed, effect } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { Card } from 'primeng/card';
import { Tag } from 'primeng/tag';
import { ProgressBar } from 'primeng/progressbar';
import { Button } from 'primeng/button';
import { MessageService } from 'primeng/api';
import { ExamService } from '../../core/services/exam.service';
import { ExamResultResponse } from '../../core/models';

@Component({
  selector: 'app-exam-result-detail',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Card, Tag, ProgressBar, Button, DatePipe],
  templateUrl: './exam-result-detail.component.html',
  styleUrl: './exam-result-detail.component.scss',
})
export class ExamResultDetailComponent {
  private readonly examService = inject(ExamService);
  private readonly messageService = inject(MessageService);
  private readonly router = inject(Router);

  readonly examId = input.required<string>();

  readonly result = signal<ExamResultResponse | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly scorePercentage = computed(() => {
    const r = this.result();
    return r && r.totalMarks > 0
      ? Math.round(((r.totalScore ?? 0) / r.totalMarks) * 100)
      : 0;
  });

  constructor() {
    effect(() => {
      const id = this.examId();
      if (id) {
        this.loadResult(+id);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/my-results']);
  }

  private loadResult(examId: number): void {
    this.loading.set(true);
    this.error.set(null);
    this.examService.getMyExamResult(examId).subscribe({
      next: (response) => {
        this.result.set(response);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load exam result.');
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load exam result.',
        });
        this.loading.set(false);
      },
    });
  }
}
