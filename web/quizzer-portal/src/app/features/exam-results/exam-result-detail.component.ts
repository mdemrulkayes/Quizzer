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
  template: `
    @if (loading()) {
      <div class="loading-container">
        <i class="pi pi-spin pi-spinner" style="font-size: 2rem;"></i>
        <p>Loading exam result...</p>
      </div>
    } @else if (error()) {
      <div class="error-container">
        <i class="pi pi-exclamation-triangle" style="font-size: 2rem; color: var(--red-500);"></i>
        <p>{{ error() }}</p>
        <p-button label="Go Back" icon="pi pi-arrow-left" severity="secondary" (onClick)="goBack()" />
      </div>
    } @else if (result(); as r) {
      <div class="result-page">
        <div class="header-row">
          <p-button label="Back to Results" icon="pi pi-arrow-left" severity="secondary" [text]="true" (onClick)="goBack()" />
        </div>

        <p-card>
          <div class="score-card">
            <div class="score-header">
              <h2 class="exam-title">{{ r.examTitle }}</h2>
              @if (r.isPassed === true) {
                <p-tag value="Passed" severity="success" [rounded]="true" />
              } @else if (r.isPassed === false) {
                <p-tag value="Failed" severity="danger" [rounded]="true" />
              } @else {
                <p-tag value="Pending" severity="warn" [rounded]="true" />
              }
            </div>

            <div class="score-details">
              <div class="score-item">
                <span class="score-label">Score</span>
                <span class="score-value">{{ r.totalScore ?? 0 }} / {{ r.totalMarks }}</span>
              </div>
              <div class="score-item">
                <span class="score-label">Passing Marks</span>
                <span class="score-value">{{ r.passingMarks }}</span>
              </div>
              <div class="score-item">
                <span class="score-label">Submitted</span>
                <span class="score-value">{{ r.submittedAt ? (r.submittedAt | date: 'medium') : 'Not submitted' }}</span>
              </div>
              <div class="score-item">
                <span class="score-label">Status</span>
                <span class="score-value">{{ r.status }}</span>
              </div>
            </div>

            <div class="progress-section">
              <span class="progress-label">{{ scorePercentage() }}%</span>
              <p-progressbar [value]="scorePercentage()" [showValue]="false" />
            </div>
          </div>
        </p-card>

        <h3 class="section-title">Question Breakdown</h3>

        @for (answer of r.answers; track answer.questionId; let i = $index) {
          <div class="question-card" [class.correct]="answer.isCorrect === true" [class.incorrect]="answer.isCorrect === false">
            <div class="question-header">
              <span class="question-number">Q{{ i + 1 }}</span>
              <span class="question-text">{{ answer.questionText }}</span>
              <span class="question-marks">
                {{ answer.marksAwarded ?? 0 }} pts
              </span>
            </div>
            <div class="question-answer">
              <span class="answer-label">Your Answer:</span>
              @if (answer.selectedOptionText) {
                <span class="answer-text">{{ answer.selectedOptionText }}</span>
              } @else {
                <span class="answer-text not-answered">Not answered</span>
              }
              @if (answer.isCorrect === true) {
                <p-tag value="Correct" severity="success" />
              } @else if (answer.isCorrect === false) {
                <p-tag value="Incorrect" severity="danger" />
              } @else {
                <p-tag value="Pending" severity="warn" />
              }
            </div>
          </div>
        }
      </div>
    }
  `,
  styles: `
    :host {
      display: block;
      padding: 1rem;
    }

    .loading-container,
    .error-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 3rem;
      gap: 1rem;
    }

    .header-row {
      margin-bottom: 1rem;
    }

    .score-card {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .score-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      flex-wrap: wrap;
      gap: 0.5rem;
    }

    .exam-title {
      margin: 0;
      font-size: 1.5rem;
    }

    .score-details {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: 1rem;
    }

    .score-item {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }

    .score-label {
      font-size: 0.85rem;
      color: var(--text-color-secondary);
      font-weight: 600;
      text-transform: uppercase;
    }

    .score-value {
      font-size: 1.1rem;
      font-weight: 500;
    }

    .progress-section {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .progress-label {
      font-weight: 600;
      font-size: 0.9rem;
    }

    .section-title {
      margin: 1.5rem 0 1rem;
    }

    .question-card {
      border: 1px solid var(--surface-border);
      border-left: 4px solid var(--surface-border);
      border-radius: 6px;
      padding: 1rem;
      margin-bottom: 0.75rem;
      background: var(--surface-card);
      transition: border-color 0.2s;
    }

    .question-card.correct {
      border-left-color: var(--green-500);
      background: color-mix(in srgb, var(--green-500) 5%, var(--surface-card));
    }

    .question-card.incorrect {
      border-left-color: var(--red-500);
      background: color-mix(in srgb, var(--red-500) 5%, var(--surface-card));
    }

    .question-header {
      display: flex;
      align-items: flex-start;
      gap: 0.75rem;
      margin-bottom: 0.75rem;
    }

    .question-number {
      font-weight: 700;
      background: var(--surface-200);
      border-radius: 4px;
      padding: 0.15rem 0.5rem;
      font-size: 0.85rem;
      white-space: nowrap;
    }

    .question-text {
      flex: 1;
      font-weight: 500;
    }

    .question-marks {
      font-weight: 600;
      white-space: nowrap;
      color: var(--text-color-secondary);
    }

    .question-answer {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding-left: 2.5rem;
      flex-wrap: wrap;
    }

    .answer-label {
      font-size: 0.85rem;
      color: var(--text-color-secondary);
    }

    .answer-text {
      font-weight: 500;
    }

    .answer-text.not-answered {
      font-style: italic;
      color: var(--text-color-secondary);
    }
  `,
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
