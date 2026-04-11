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
  template: `
    <div class="page-header">
      <h2>Question Set Detail</h2>
      <p-button label="Back to List" icon="pi pi-arrow-left" [text]="true" (onClick)="goBack()" />
    </div>

    @if (loading()) {
      <p>Loading...</p>
    } @else if (questionSet()) {
      <div class="set-info-card">
        <h3>{{ questionSet()!.name }}</h3>
        @if (questionSet()!.setCode) {
          <p><strong>Code:</strong> {{ questionSet()!.setCode }}</p>
        }
        @if (questionSet()!.details) {
          <p>{{ questionSet()!.details }}</p>
        }
      </div>

      <!-- Tags Section -->
      <div class="section-card">
        <div class="section-header">
          <h3>Tags</h3>
        </div>
        <div class="tags-list">
          @for (tag of tags(); track tag.tagId) {
            <p-chip [label]="tag.name" [removable]="true" (onRemove)="removeTag(tag)" />
          } @empty {
            <span class="text-muted">No tags assigned.</span>
          }
        </div>
        <div class="add-tag-row">
          <p-select
            [options]="availableTags()"
            [(ngModel)]="selectedTagId"
            optionLabel="name"
            optionValue="tagId"
            placeholder="Select a tag to add"
            [style]="{ minWidth: '220px' }"
          />
          <p-button label="Add Tag" icon="pi pi-plus" [disabled]="!selectedTagId" (onClick)="addTag()" size="small" />
        </div>
      </div>

      <!-- Questions Section -->
      <div class="section-card">
        <div class="section-header">
          <h3>Questions ({{ questionSet()!.questions.length }})</h3>
          <p-button label="Add Question" icon="pi pi-plus" (onClick)="openNewQuestion()" />
        </div>
        <div class="questions-list">
          @for (q of questionSet()!.questions; track q.questionId; let i = $index) {
            <p-panel
              [header]="'Q' + (i + 1) + ': ' + truncate(q.question, 80)"
              [toggleable]="true"
              [collapsed]="true"
            >
              <div class="question-content">
                <div class="question-meta">
                  <p><strong>Question:</strong> {{ q.question }}</p>
                  @if (q.details) {
                    <p><strong>Details:</strong> {{ q.details }}</p>
                  }
                  <p><strong>Marks:</strong> {{ q.mark ?? 'N/A' }}</p>
                </div>
                <h4>Options</h4>
                <div class="options-list">
                  @for (opt of q.questionOptions; track opt.questionOptionId) {
                    <div class="option-item" [class.correct]="opt.isCorrect">
                      <i [class]="opt.isCorrect ? 'pi pi-check-circle' : 'pi pi-circle'"></i>
                      <span>{{ opt.optionText }}</span>
                      @if (opt.isCorrect) {
                        <p-tag value="Correct" severity="success" />
                      }
                    </div>
                  } @empty {
                    <p class="text-muted">No options defined.</p>
                  }
                </div>
                <div class="question-actions">
                  <p-button icon="pi pi-pencil" label="Edit" severity="warn" [text]="true" (onClick)="editQuestion(q)" />
                  <p-button icon="pi pi-trash" label="Delete" severity="danger" [text]="true" (onClick)="confirmDeleteQuestion(q)" />
                </div>
              </div>
            </p-panel>
          } @empty {
            <p class="text-muted">No questions yet. Click "Add Question" to get started.</p>
          }
        </div>
      </div>
    }

    <!-- Question Form Dialog -->
    <p-dialog
      [header]="editingQuestion() ? 'Edit Question' : 'New Question'"
      [(visible)]="displayQuestionDialog"
      [modal]="true"
      [style]="{ width: '650px' }"
      [closable]="true"
    >
      <form [formGroup]="questionForm" (ngSubmit)="saveQuestion()">
        <div class="form-grid">
          <div class="form-field">
            <label for="qText">Question</label>
            <textarea id="qText" pTextarea formControlName="question" rows="3" class="w-full"></textarea>
          </div>
          <div class="form-field">
            <label for="qDetails">Details</label>
            <textarea id="qDetails" pTextarea formControlName="details" rows="2" class="w-full"></textarea>
          </div>
          <div class="form-field">
            <label for="qMark">Marks</label>
            <p-inputnumber id="qMark" formControlName="mark" [min]="0" [showButtons]="true" [style]="{ width: '100%' }" />
          </div>

          <div class="options-section">
            <div class="options-header">
              <label>Options (select the correct answer)</label>
              <p-button label="Add Option" icon="pi pi-plus" [text]="true" size="small" (onClick)="addOption()" />
            </div>
            <div formArrayName="options">
              @for (option of optionsFormArray.controls; track $index; let i = $index) {
                <div class="option-row" [formGroupName]="i">
                  <p-radiobutton
                    name="correctAnswer"
                    [value]="i"
                    [(ngModel)]="selectedAnswerIdx"
                    [ngModelOptions]="{ standalone: true }"
                    [inputId]="'answer_' + i"
                  />
                  <input pInputText formControlName="optionText" placeholder="Option text" class="flex-1" />
                  <p-button
                    icon="pi pi-trash"
                    [text]="true"
                    [rounded]="true"
                    severity="danger"
                    size="small"
                    (onClick)="removeOption(i)"
                  />
                </div>
              }
            </div>
          </div>
        </div>
        <div class="dialog-actions">
          <p-button label="Cancel" severity="secondary" [text]="true" (onClick)="displayQuestionDialog = false" />
          <p-button
            label="Save"
            icon="pi pi-check"
            type="submit"
            [disabled]="questionForm.invalid || optionsFormArray.length === 0"
          />
        </div>
      </form>
    </p-dialog>

    <p-confirmDialog />
  `,
  styles: [`
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
    }

    .page-header h2 {
      margin: 0;
      color: var(--p-text-color);
      font-size: 1.5rem;
    }

    .set-info-card {
      background: var(--p-surface-0);
      border: 1px solid var(--p-surface-200);
      border-radius: var(--p-border-radius);
      padding: 1.5rem;
      margin-bottom: 1.5rem;
    }

    .set-info-card h3 {
      margin: 0 0 0.5rem;
      color: var(--p-text-color);
    }

    .set-info-card p {
      margin: 0.25rem 0;
      color: var(--p-text-muted-color);
    }

    .section-card {
      background: var(--p-surface-0);
      border: 1px solid var(--p-surface-200);
      border-radius: var(--p-border-radius);
      padding: 1.5rem;
      margin-bottom: 1.5rem;
    }

    .section-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
    }

    .section-header h3 {
      margin: 0;
      color: var(--p-text-color);
    }

    .tags-list {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      margin-bottom: 1rem;
    }

    .add-tag-row {
      display: flex;
      gap: 0.5rem;
      align-items: center;
    }

    .questions-list {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .question-content {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .question-meta p {
      margin: 0.25rem 0;
    }

    .question-content h4 {
      margin: 0.5rem 0 0.25rem;
      color: var(--p-text-color);
    }

    .options-list {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .option-item {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.5rem 0.75rem;
      border-radius: var(--p-border-radius);
      background: var(--p-surface-50);
    }

    .option-item.correct {
      background: var(--p-green-50);
      color: var(--p-green-700);
    }

    .option-item i {
      font-size: 1rem;
    }

    .question-actions {
      display: flex;
      gap: 0.5rem;
      border-top: 1px solid var(--p-surface-200);
      padding-top: 0.75rem;
      margin-top: 0.25rem;
    }

    .form-grid {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .form-field {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .form-field label {
      font-weight: 600;
      color: var(--p-text-color);
    }

    .options-section {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .options-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .options-header label {
      font-weight: 600;
      color: var(--p-text-color);
    }

    .option-row {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .flex-1 {
      flex: 1;
    }

    .dialog-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.5rem;
      margin-top: 1.5rem;
    }

    .w-full {
      width: 100%;
    }

    .text-muted {
      color: var(--p-text-muted-color);
    }
  `],
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
