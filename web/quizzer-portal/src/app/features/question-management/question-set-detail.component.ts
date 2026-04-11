import {
  Component, ChangeDetectionStrategy, inject, signal, computed, input, effect, OnInit,
} from '@angular/core';
import { Router } from '@angular/router';
import {
  FormsModule, ReactiveFormsModule, FormBuilder, FormArray, FormGroup, FormControl, Validators,
} from '@angular/forms';
import { forkJoin } from 'rxjs';
import { QuizService } from '../../core/services/quiz.service';
import {
  QuestionSetResponse, QuestionResponse, QuestionOptionResponse, TagResponse,
  CreateQuestionRequest, CreateQuestionOptionRequest,
} from '../../core/models';
import { MessageService, ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import { InputNumber } from 'primeng/inputnumber';
import { Select } from 'primeng/select';
import { Chip } from 'primeng/chip';
import { Tag } from 'primeng/tag';
import { Panel } from 'primeng/panel';
import { RadioButton } from 'primeng/radiobutton';
import { ConfirmDialog } from 'primeng/confirmdialog';

@Component({
  selector: 'app-question-set-detail',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    Button,
    Dialog,
    InputText,
    Textarea,
    InputNumber,
    Select,
    Chip,
    Tag,
    Panel,
    RadioButton,
    ConfirmDialog,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './question-set-detail.component.html',
  styleUrl: './question-set-detail.component.scss',
})
export class QuestionSetDetailComponent implements OnInit {
  private readonly quizService = inject(QuizService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);

  readonly setId = input.required<string>();

  readonly questionSet = signal<QuestionSetResponse | null>(null);
  readonly tags = signal<TagResponse[]>([]);
  readonly allTags = signal<TagResponse[]>([]);
  readonly loading = signal(true);
  readonly editingQuestion = signal<QuestionResponse | null>(null);

  readonly availableTags = computed(() => {
    const assigned = new Set(this.tags().map(t => t.tagId));
    return this.allTags().filter(t => !assigned.has(t.tagId));
  });

  displayQuestionDialog = false;
  selectedTagId: number | null = null;
  selectedAnswerIdx = 0;

  questionForm = this.fb.group({
    question: ['', Validators.required],
    details: [''],
    mark: [null as number | null],
    options: this.fb.array<FormGroup>([]),
  });

  get optionsFormArray(): FormArray<FormGroup> {
    return this.questionForm.controls.options;
  }

  constructor() {
    effect(() => {
      const id = this.setId();
      if (id) {
        this.loadData(+id);
      }
    });
  }

  ngOnInit(): void {
    this.quizService.getTags({ pageSize: 200 }).subscribe({
      next: (res) => this.allTags.set(res.items),
    });
  }

  goBack(): void {
    this.router.navigate(['/question-sets']);
  }

  // --- Tag Management ---

  addTag(): void {
    if (!this.selectedTagId) return;
    const setId = +this.setId();
    this.quizService.assignTagToQuestionSet(setId, { tagId: this.selectedTagId }).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Tag assigned.' });
        this.selectedTagId = null;
        this.loadTags(setId);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to assign tag.' });
      },
    });
  }

  removeTag(tag: TagResponse): void {
    const setId = +this.setId();
    this.quizService.removeTagFromQuestionSet(setId, tag.tagId).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Removed', detail: 'Tag removed.' });
        this.loadTags(setId);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to remove tag.' });
      },
    });
  }

  // --- Question Management ---

  openNewQuestion(): void {
    this.editingQuestion.set(null);
    this.questionForm.reset({ question: '', details: '', mark: null });
    this.optionsFormArray.clear();
    this.selectedAnswerIdx = 0;
    this.addOption();
    this.addOption();
    this.displayQuestionDialog = true;
  }

  editQuestion(q: QuestionResponse): void {
    this.editingQuestion.set(q);
    this.questionForm.patchValue({
      question: q.question,
      details: q.details,
      mark: q.mark,
    });
    this.optionsFormArray.clear();
    this.selectedAnswerIdx = 0;
    q.questionOptions.forEach((opt, i) => {
      this.optionsFormArray.push(this.fb.group({
        optionText: [opt.optionText, Validators.required],
        questionOptionId: [opt.questionOptionId],
      }));
      if (opt.isCorrect) this.selectedAnswerIdx = i;
    });
    this.displayQuestionDialog = true;
  }

  addOption(): void {
    this.optionsFormArray.push(this.fb.group({
      optionText: ['', Validators.required],
      questionOptionId: [null as number | null],
    }));
  }

  removeOption(index: number): void {
    this.optionsFormArray.removeAt(index);
    if (this.selectedAnswerIdx >= this.optionsFormArray.length) {
      this.selectedAnswerIdx = Math.max(0, this.optionsFormArray.length - 1);
    }
  }

  saveQuestion(): void {
    if (this.questionForm.invalid || this.optionsFormArray.length === 0) return;
    const { question, details, mark } = this.questionForm.value;
    const options = this.optionsFormArray.value;
    const editing = this.editingQuestion();

    if (editing) {
      this.quizService.updateQuestion(editing.questionId, {
        questionId: editing.questionId,
        question: question!,
        details: details ?? '',
        mark: mark ?? null,
      }).subscribe({
        next: () => this.reconcileOptions(editing.questionId, editing.questionOptions, options),
        error: () => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update question.' });
        },
      });
    } else {
      const optionRequests: CreateQuestionOptionRequest[] = options.map((o: any, i: number) => ({
        optionText: o.optionText,
        isAnswer: i === this.selectedAnswerIdx,
      }));
      const payload = {
        question: question!,
        details: details ?? '',
        mark: mark ?? null,
        questionOptions: optionRequests,
        questionSetId: +this.setId(),
      };
      this.quizService.createQuestion(payload as CreateQuestionRequest).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Question created.' });
          this.displayQuestionDialog = false;
          this.loadData(+this.setId());
        },
        error: () => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to create question.' });
        },
      });
    }
  }

  confirmDeleteQuestion(q: QuestionResponse): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete this question?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.quizService.deleteQuestion(q.questionId).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Deleted', detail: 'Question deleted.' });
            this.loadData(+this.setId());
          },
          error: () => {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete question.' });
          },
        });
      },
    });
  }

  truncate(text: string | null, max: number): string {
    if (!text) return '\u2014';
    return text.length > max ? text.substring(0, max) + '...' : text;
  }

  // --- Private Helpers ---

  private loadData(setId: number): void {
    this.loading.set(true);
    this.quizService.getQuestionSetById(setId).subscribe({
      next: (qs) => {
        this.questionSet.set(qs);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load question set.' });
      },
    });
    this.loadTags(setId);
  }

  private loadTags(setId: number): void {
    this.quizService.getQuestionSetTags(setId).subscribe({
      next: (tags) => this.tags.set(tags),
    });
  }

  private reconcileOptions(
    questionId: number,
    existing: QuestionOptionResponse[],
    formOptions: any[],
  ): void {
    const ops: ReturnType<typeof this.quizService.addOption>[] = [];
    const formOptionIds = new Set(
      formOptions.map((o: any) => o.questionOptionId).filter((id: number | null) => id != null),
    );

    for (const ex of existing) {
      if (!formOptionIds.has(ex.questionOptionId)) {
        ops.push(this.quizService.deleteOption(questionId, ex.questionOptionId));
      }
    }

    for (let i = 0; i < formOptions.length; i++) {
      const opt = formOptions[i];
      const isAnswer = i === this.selectedAnswerIdx;
      if (opt.questionOptionId) {
        ops.push(this.quizService.updateOption(questionId, opt.questionOptionId, {
          optionText: opt.optionText,
          isAnswer,
        }));
      } else {
        ops.push(this.quizService.addOption(questionId, {
          optionText: opt.optionText,
          isAnswer,
        }));
      }
    }

    if (ops.length === 0) {
      this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Question updated.' });
      this.displayQuestionDialog = false;
      this.loadData(+this.setId());
      return;
    }

    forkJoin(ops).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Question updated.' });
        this.displayQuestionDialog = false;
        this.loadData(+this.setId());
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update options.' });
        this.loadData(+this.setId());
      },
    });
  }
}
