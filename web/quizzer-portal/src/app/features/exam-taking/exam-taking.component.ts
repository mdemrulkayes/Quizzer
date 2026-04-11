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
  templateUrl: './exam-taking.component.html',
  styleUrl: './exam-taking.component.scss',
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
