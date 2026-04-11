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
  templateUrl: './question-list.component.html',
  styleUrl: './question-list.component.scss',
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
