import {
  Component,
  ChangeDetectionStrategy,
  DestroyRef,
  inject,
  signal,
  computed,
  linkedSignal,
  OnInit,
} from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Card } from 'primeng/card';
import { RadioButton } from 'primeng/radiobutton';
import { Button } from 'primeng/button';
import { ProgressBar } from 'primeng/progressbar';
import { Tag } from 'primeng/tag';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ExamService } from '../../core/services/exam.service';
import {
  ExamAttemptStartResponse,
  ExamQuestionResponse,
} from '../../core/models';

@Component({
  selector: 'app-exam-taking',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    Card,
    RadioButton,
    Button,
    ProgressBar,
    Tag,
    ConfirmDialog,
  ],
  providers: [ConfirmationService],
  template: `
    @if (attempt()) {
      <div class="exam-taking-container">
        <!-- Timer Bar -->
        <div class="timer-bar">
          <p-card>
            <div class="timer-bar-content">
              <h2 class="exam-title">{{ attempt()!.examTitle }}</h2>
              <div class="timer-section">
                <p-tag
                  [severity]="remainingSeconds() < 60 ? 'danger' : remainingSeconds() < 300 ? 'warn' : 'info'"
                  [value]="'⏱ ' + formattedTime()"
                  styleClass="timer-tag"
                />
                <span class="question-counter">
                  Question {{ currentQuestionIndex() + 1 }} of {{ attempt()!.questions.length }}
                </span>
              </div>
            </div>
            <p-progressbar
              [value]="timerPercentage()"
              [showValue]="false"
              styleClass="timer-progress"
            />
          </p-card>
        </div>

        <div class="exam-body">
          <!-- Question Area -->
          <div class="question-area">
            <p-card>
              @if (currentQuestion(); as question) {
                <div class="question-header">
                  <h3>Question {{ currentQuestionIndex() + 1 }}</h3>
                  @if (question.marks !== null) {
                    <p-tag severity="info" [value]="question.marks + ' marks'" />
                  }
                </div>
                <p class="question-text">{{ question.questionText }}</p>

                <div class="options-list">
                  @for (option of question.options; track option.optionId) {
                    <div class="option-item">
                      <p-radiobutton
                        name="question"
                        [value]="option.optionId"
                        [ngModel]="currentAnswer()"
                        (ngModelChange)="selectOption(option.optionId)"
                        [inputId]="'opt-' + option.optionId"
                      />
                      <label [for]="'opt-' + option.optionId" class="option-label">
                        {{ option.optionText }}
                      </label>
                    </div>
                  }
                </div>

                <div class="navigation-buttons">
                  <p-button
                    label="Previous"
                    icon="pi pi-arrow-left"
                    [disabled]="currentQuestionIndex() === 0"
                    severity="secondary"
                    (onClick)="goToQuestion(currentQuestionIndex() - 1)"
                  />
                  @if (currentQuestionIndex() < attempt()!.questions.length - 1) {
                    <p-button
                      label="Next"
                      icon="pi pi-arrow-right"
                      iconPos="right"
                      (onClick)="goToQuestion(currentQuestionIndex() + 1)"
                    />
                  } @else {
                    <p-button
                      label="Submit Exam"
                      icon="pi pi-check"
                      severity="success"
                      (onClick)="confirmSubmit()"
                    />
                  }
                </div>
              }
            </p-card>
          </div>

          <!-- Question Navigation Sidebar -->
          <div class="question-nav">
            <p-card header="Questions">
              <div class="nav-grid">
                @for (question of attempt()!.questions; track question.questionId; let i = $index) {
                  <p-button
                    [label]="'' + (i + 1)"
                    [severity]="getQuestionSeverity(i, question.questionId)"
                    [outlined]="i !== currentQuestionIndex()"
                    styleClass="nav-btn"
                    (onClick)="goToQuestion(i)"
                  />
                }
              </div>
              <div class="nav-legend">
                <span class="legend-item"><span class="dot dot-info"></span> Current</span>
                <span class="legend-item"><span class="dot dot-success"></span> Answered</span>
                <span class="legend-item"><span class="dot dot-secondary"></span> Unanswered</span>
              </div>
              <div class="nav-summary">
                <p>Answered: {{ answeredCount() }} / {{ attempt()!.questions.length }}</p>
              </div>
              <p-button
                label="Submit Exam"
                icon="pi pi-check"
                severity="success"
                styleClass="submit-btn"
                (onClick)="confirmSubmit()"
              />
            </p-card>
          </div>
        </div>
      </div>

      <p-confirmdialog />
    }
  `,
  styles: `
    .exam-taking-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 1rem;
    }

    .timer-bar {
      margin-bottom: 1rem;
    }

    .timer-bar-content {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 0.5rem;
      margin-bottom: 0.5rem;
    }

    .exam-title {
      margin: 0;
      font-size: 1.25rem;
    }

    .timer-section {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    :host ::ng-deep .timer-tag {
      font-size: 1.1rem;
      padding: 0.4rem 0.8rem;
    }

    .question-counter {
      font-weight: 500;
      color: var(--text-color-secondary);
    }

    :host ::ng-deep .timer-progress .p-progressbar {
      height: 6px;
    }

    .exam-body {
      display: flex;
      gap: 1rem;
      align-items: flex-start;
    }

    .question-area {
      flex: 1;
      min-width: 0;
    }

    .question-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
    }

    .question-header h3 {
      margin: 0;
    }

    .question-text {
      font-size: 1.1rem;
      line-height: 1.6;
      margin-bottom: 1.5rem;
    }

    .options-list {
      display: flex;
      flex-direction: column;
      gap: 1rem;
      margin-bottom: 2rem;
    }

    .option-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.75rem 1rem;
      border: 1px solid var(--surface-border);
      border-radius: var(--border-radius);
      cursor: pointer;
      transition: background-color 0.2s;
    }

    .option-item:hover {
      background-color: var(--surface-hover);
    }

    .option-label {
      cursor: pointer;
      flex: 1;
    }

    .navigation-buttons {
      display: flex;
      justify-content: space-between;
      padding-top: 1rem;
      border-top: 1px solid var(--surface-border);
    }

    .question-nav {
      width: 280px;
      flex-shrink: 0;
    }

    .nav-grid {
      display: grid;
      grid-template-columns: repeat(5, 1fr);
      gap: 0.5rem;
      margin-bottom: 1rem;
    }

    :host ::ng-deep .nav-btn {
      width: 100%;
      aspect-ratio: 1;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .nav-legend {
      display: flex;
      flex-wrap: wrap;
      gap: 0.75rem;
      margin-bottom: 0.75rem;
      font-size: 0.85rem;
    }

    .legend-item {
      display: flex;
      align-items: center;
      gap: 0.35rem;
    }

    .dot {
      width: 12px;
      height: 12px;
      border-radius: 50%;
      display: inline-block;
    }

    .dot-info {
      background-color: var(--p-blue-500, var(--blue-500));
    }

    .dot-success {
      background-color: var(--p-green-500, var(--green-500));
    }

    .dot-secondary {
      background-color: var(--p-gray-400, var(--gray-400));
    }

    .nav-summary {
      margin-bottom: 1rem;
      font-weight: 500;
    }

    :host ::ng-deep .submit-btn {
      width: 100%;
    }

    @media (max-width: 768px) {
      .exam-body {
        flex-direction: column;
      }

      .question-nav {
        width: 100%;
        order: -1;
      }
    }
  `,
})
export class ExamTakingComponent implements OnInit {
  private readonly examService = inject(ExamService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly attempt = signal<ExamAttemptStartResponse | null>(null);
  readonly remainingSeconds = signal(0);
  readonly answers = signal(new Map<number, number>());

  readonly currentQuestionIndex = linkedSignal({
    source: () => this.attempt(),
    computation: () => 0,
  });

  readonly currentQuestion = computed<ExamQuestionResponse | null>(() => {
    const att = this.attempt();
    if (!att) return null;
    return att.questions[this.currentQuestionIndex()] ?? null;
  });

  readonly currentAnswer = computed(() => {
    const q = this.currentQuestion();
    if (!q) return null;
    return this.answers().get(q.questionId) ?? null;
  });

  readonly totalDurationSeconds = computed(() => {
    const att = this.attempt();
    return att ? att.durationInMinutes * 60 : 0;
  });

  readonly timerPercentage = computed(() => {
    const total = this.totalDurationSeconds();
    if (total === 0) return 0;
    return Math.round((this.remainingSeconds() / total) * 100);
  });

  readonly formattedTime = computed(() => {
    const s = this.remainingSeconds();
    const mins = Math.floor(s / 60).toString().padStart(2, '0');
    const secs = (s % 60).toString().padStart(2, '0');
    return `${mins}:${secs}`;
  });

  readonly answeredCount = computed(() => this.answers().size);

  private timerInterval: ReturnType<typeof setInterval> | null = null;
  private submitting = false;

  ngOnInit(): void {
    const attemptData = history.state?.attempt as ExamAttemptStartResponse | undefined;

    if (!attemptData) {
      this.router.navigate(['/available-exams']);
      return;
    }

    this.attempt.set(attemptData);
    this.startTimer(attemptData.expiresAt);
  }

  private startTimer(expiresAt: string): void {
    const expiresTime = new Date(expiresAt).getTime();

    const updateRemaining = () => {
      const now = Date.now();
      const diff = Math.max(0, Math.floor((expiresTime - now) / 1000));
      this.remainingSeconds.set(diff);

      if (diff <= 0 && !this.submitting) {
        this.autoSubmit();
      }
    };

    updateRemaining();

    this.timerInterval = setInterval(updateRemaining, 1000);

    this.destroyRef.onDestroy(() => {
      if (this.timerInterval !== null) {
        clearInterval(this.timerInterval);
      }
    });
  }

  goToQuestion(index: number): void {
    const att = this.attempt();
    if (!att) return;
    if (index >= 0 && index < att.questions.length) {
      this.currentQuestionIndex.set(index);
    }
  }

  selectOption(optionId: number): void {
    const att = this.attempt();
    const q = this.currentQuestion();
    if (!att || !q) return;

    this.answers.update(m => {
      const copy = new Map(m);
      copy.set(q.questionId, optionId);
      return copy;
    });

    this.examService
      .submitAnswer(att.examId, {
        questionId: q.questionId,
        selectedOptionId: optionId,
      })
      .subscribe({
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to save answer. Please try again.',
          });
        },
      });
  }

  getQuestionSeverity(
    index: number,
    questionId: number,
  ): 'success' | 'info' | 'secondary' {
    if (index === this.currentQuestionIndex()) return 'info';
    if (this.answers().has(questionId)) return 'success';
    return 'secondary';
  }

  confirmSubmit(): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to submit this exam? You cannot change your answers after submission.',
      header: 'Submit Exam',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Submit',
      rejectLabel: 'Cancel',
      accept: () => this.submitExam(),
    });
  }

  private submitExam(): void {
    const att = this.attempt();
    if (!att || this.submitting) return;
    this.submitting = true;

    this.examService.submitExam(att.examId).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Submitted',
          detail: 'Exam submitted successfully!',
        });
        this.router.navigate([`/exam/${att.examId}/result`]);
      },
      error: () => {
        this.submitting = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to submit exam. Please try again.',
        });
      },
    });
  }

  private autoSubmit(): void {
    if (this.submitting) return;
    this.messageService.add({
      severity: 'warn',
      summary: 'Time Up',
      detail: 'Time has expired. Submitting your exam automatically.',
    });
    this.submitExam();
  }
}
