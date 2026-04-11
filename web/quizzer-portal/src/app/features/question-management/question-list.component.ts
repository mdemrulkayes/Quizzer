import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { QuizService } from '../../core/services/quiz.service';
import { QuestionResponse, QuestionSetResponse } from '../../core/models';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import { InputNumber } from 'primeng/inputnumber';
import { Select } from 'primeng/select';
import { Tag } from 'primeng/tag';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Toolbar } from 'primeng/toolbar';

@Component({
  selector: 'app-question-list',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    TableModule,
    Button,
    Dialog,
    InputText,
    Textarea,
    InputNumber,
    Select,
    Tag,
    ConfirmDialog,
    IconField,
    InputIcon,
    Toolbar,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <h2>Questions</h2>
    </div>

    <p-toolbar>
      <ng-template #start>
        <div class="toolbar-filters">
          <p-select
            [options]="questionSetOptions()"
            [(ngModel)]="selectedSetId"
            optionLabel="name"
            optionValue="questionSetId"
            placeholder="Filter by question set"
            [showClear]="true"
            (onChange)="onSetFilterChange()"
            [style]="{ minWidth: '220px' }"
          />
        </div>
      </ng-template>
      <ng-template #end>
        <p-iconfield>
          <p-inputicon styleClass="pi pi-search" />
          <input pInputText placeholder="Search questions..." [(ngModel)]="searchTerm" (input)="onSearch()" />
        </p-iconfield>
      </ng-template>
    </p-toolbar>

    <p-table
      [value]="questions()"
      [lazy]="true"
      [paginator]="true"
      [rows]="pageSize"
      [totalRecords]="totalRecords()"
      [loading]="loading()"
      (onLazyLoad)="onLazyLoad($event)"
      [rowHover]="true"
      dataKey="questionId"
      styleClass="p-datatable-sm"
    >
      <ng-template #header>
        <tr>
          <th style="width: 3rem"></th>
          <th>Question</th>
          <th style="width: 8rem">Marks</th>
          <th style="width: 8rem">Options</th>
          <th style="width: 10rem">Actions</th>
        </tr>
      </ng-template>
      <ng-template #body let-q let-expanded="expanded">
        <tr>
          <td>
            <p-button
              type="button"
              [pRowToggler]="q"
              [icon]="expanded ? 'pi pi-chevron-down' : 'pi pi-chevron-right'"
              [text]="true"
              [rounded]="true"
              size="small"
            />
          </td>
          <td>{{ truncate(q.question, 80) }}</td>
          <td>{{ q.mark ?? 'N/A' }}</td>
          <td>{{ q.questionOptions?.length ?? 0 }}</td>
          <td>
            <p-button icon="pi pi-pencil" [text]="true" [rounded]="true" severity="warn" (onClick)="editQuestion(q)" />
            <p-button icon="pi pi-trash" [text]="true" [rounded]="true" severity="danger" (onClick)="confirmDelete(q)" />
          </td>
        </tr>
      </ng-template>
      <ng-template #rowexpansion let-q>
        <tr>
          <td colspan="5">
            <div class="expansion-content">
              @if (q.details) {
                <p class="question-details"><strong>Details:</strong> {{ q.details }}</p>
              }
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
            </div>
          </td>
        </tr>
      </ng-template>
      <ng-template #emptymessage>
        <tr>
          <td colspan="5" class="text-center p-4">No questions found.</td>
        </tr>
      </ng-template>
    </p-table>

    <!-- Edit Question Dialog -->
    <p-dialog
      header="Edit Question"
      [(visible)]="displayDialog"
      [modal]="true"
      [style]="{ width: '550px' }"
      [closable]="true"
    >
      <form [formGroup]="questionForm" (ngSubmit)="saveQuestion()">
        <div class="form-grid">
          <div class="form-field">
            <label for="editQText">Question</label>
            <textarea id="editQText" pTextarea formControlName="question" rows="3" class="w-full"></textarea>
          </div>
          <div class="form-field">
            <label for="editQDetails">Details</label>
            <textarea id="editQDetails" pTextarea formControlName="details" rows="2" class="w-full"></textarea>
          </div>
          <div class="form-field">
            <label for="editQMark">Marks</label>
            <p-inputnumber id="editQMark" formControlName="mark" [min]="0" [showButtons]="true" [style]="{ width: '100%' }" />
          </div>
        </div>
        <div class="dialog-actions">
          <p-button label="Cancel" severity="secondary" [text]="true" (onClick)="displayDialog = false" />
          <p-button label="Save" icon="pi pi-check" type="submit" [disabled]="questionForm.invalid" />
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

    p-toolbar {
      display: block;
      margin-bottom: 1rem;
    }

    .toolbar-filters {
      display: flex;
      gap: 0.75rem;
      align-items: center;
    }

    .expansion-content {
      padding: 1rem 2rem;
    }

    .expansion-content h4 {
      margin: 0.75rem 0 0.5rem;
      color: var(--p-text-color);
    }

    .question-details {
      margin: 0 0 0.5rem;
      color: var(--p-text-muted-color);
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

    .dialog-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.5rem;
      margin-top: 1.5rem;
    }

    .w-full {
      width: 100%;
    }

    .text-center {
      text-align: center;
    }

    .text-muted {
      color: var(--p-text-muted-color);
    }
  `],
})
export class QuestionListComponent implements OnInit {
  private readonly quizService = inject(QuizService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly fb = inject(FormBuilder);

  readonly questions = signal<QuestionResponse[]>([]);
  readonly totalRecords = signal(0);
  readonly loading = signal(true);
  readonly questionSetOptions = signal<QuestionSetResponse[]>([]);

  displayDialog = false;
  editingQuestionId: number | null = null;
  searchTerm = '';
  selectedSetId: number | null = null;
  pageSize = 10;
  private currentPage = 1;
  private searchTimeout: ReturnType<typeof setTimeout> | null = null;

  questionForm = this.fb.group({
    question: ['', Validators.required],
    details: [''],
    mark: [null as number | null],
  });

  ngOnInit(): void {
    this.loadQuestions();
    this.loadQuestionSets();
  }

  onLazyLoad(event: TableLazyLoadEvent): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? this.pageSize;
    this.currentPage = Math.floor(first / rows) + 1;
    this.pageSize = rows;
    this.loadQuestions();
  }

  onSearch(): void {
    if (this.searchTimeout) clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.currentPage = 1;
      this.loadQuestions();
    }, 400);
  }

  onSetFilterChange(): void {
    this.currentPage = 1;
    this.loadQuestions();
  }

  editQuestion(q: QuestionResponse): void {
    this.editingQuestionId = q.questionId;
    this.questionForm.patchValue({
      question: q.question,
      details: q.details,
      mark: q.mark,
    });
    this.displayDialog = true;
  }

  saveQuestion(): void {
    if (this.questionForm.invalid || !this.editingQuestionId) return;
    const { question, details, mark } = this.questionForm.value;
    this.quizService.updateQuestion(this.editingQuestionId, {
      questionId: this.editingQuestionId,
      question: question!,
      details: details ?? '',
      mark: mark ?? null,
    }).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Question updated.' });
        this.displayDialog = false;
        this.loadQuestions();
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update question.' });
      },
    });
  }

  confirmDelete(q: QuestionResponse): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete this question?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.quizService.deleteQuestion(q.questionId).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Deleted', detail: 'Question deleted.' });
            this.loadQuestions();
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

  private loadQuestions(): void {
    this.loading.set(true);
    this.quizService.getQuestions({
      searchText: this.searchTerm || undefined,
      questionSetId: this.selectedSetId ?? undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
    }).subscribe({
      next: (response) => {
        this.questions.set(response.items);
        this.totalRecords.set(response.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }

  private loadQuestionSets(): void {
    this.quizService.getQuestionSets({ pageSize: 100 }).subscribe({
      next: (response) => this.questionSetOptions.set(response.items),
    });
  }
}
