import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { QuizService } from '../../core/services/quiz.service';
import { QuestionSetResponse, TagResponse } from '../../core/models';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import { Select } from 'primeng/select';
import { Toolbar } from 'primeng/toolbar';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';

@Component({
  selector: 'app-question-set-list',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    TableModule,
    Button,
    Dialog,
    InputText,
    Textarea,
    Select,
    Toolbar,
    ConfirmDialog,
    IconField,
    InputIcon,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './question-set-list.component.html',
  styleUrl: './question-set-list.component.scss',
})
export class QuestionSetListComponent implements OnInit {
  private readonly quizService = inject(QuizService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly questionSets = signal<QuestionSetResponse[]>([]);
  readonly totalRecords = signal(0);
  readonly loading = signal(true);
  readonly editingSet = signal<QuestionSetResponse | null>(null);
  readonly tagOptions = signal<TagResponse[]>([]);

  displayDialog = false;
  searchTerm = '';
  selectedTagId: number | null = null;
  pageSize = 10;
  private currentPage = 1;
  private searchTimeout: ReturnType<typeof setTimeout> | null = null;

  setForm = this.fb.group({
    name: ['', Validators.required],
    setCode: [''],
    details: [''],
  });

  ngOnInit(): void {
    this.loadQuestionSets();
    this.loadTags();
  }

  onLazyLoad(event: TableLazyLoadEvent): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? this.pageSize;
    this.currentPage = Math.floor(first / rows) + 1;
    this.pageSize = rows;
    this.loadQuestionSets();
  }

  onSearch(): void {
    if (this.searchTimeout) clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.currentPage = 1;
      this.loadQuestionSets();
    }, 400);
  }

  onTagFilterChange(): void {
    this.currentPage = 1;
    this.loadQuestionSets();
  }

  openNew(): void {
    this.editingSet.set(null);
    this.setForm.reset();
    this.displayDialog = true;
  }

  viewSet(qs: QuestionSetResponse): void {
    this.router.navigate(['/question-sets', qs.questionSetId]);
  }

  editSet(qs: QuestionSetResponse): void {
    this.editingSet.set(qs);
    this.setForm.patchValue({
      name: qs.name,
      setCode: qs.setCode ?? '',
      details: qs.details ?? '',
    });
    this.displayDialog = true;
  }

  saveSet(): void {
    if (this.setForm.invalid) return;
    const { name, setCode, details } = this.setForm.value;
    const editing = this.editingSet();

    if (editing) {
      this.quizService.updateQuestionSet(editing.questionSetId, {
        questionSetId: editing.questionSetId,
        name: name!,
        setCode: setCode || null,
        details: details || null,
      }).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Question set updated.' });
          this.displayDialog = false;
          this.loadQuestionSets();
        },
        error: () => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update question set.' });
        },
      });
    } else {
      this.quizService.createQuestionSet({
        name: name!,
        setCode: setCode || null,
        details: details || null,
        questions: [],
      }).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Question set created.' });
          this.displayDialog = false;
          this.loadQuestionSets();
        },
        error: () => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to create question set.' });
        },
      });
    }
  }

  confirmDelete(qs: QuestionSetResponse): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete "${qs.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.quizService.deleteQuestionSet(qs.questionSetId).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Deleted', detail: 'Question set deleted.' });
            this.loadQuestionSets();
          },
          error: () => {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete question set.' });
          },
        });
      },
    });
  }

  truncate(text: string | null, max: number): string {
    if (!text) return '\u2014';
    return text.length > max ? text.substring(0, max) + '...' : text;
  }

  private loadQuestionSets(): void {
    this.loading.set(true);
    this.quizService.getQuestionSets({
      searchName: this.searchTerm || undefined,
      tagId: this.selectedTagId ?? undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
    }).subscribe({
      next: (response) => {
        this.questionSets.set(response.items);
        this.totalRecords.set(response.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }

  private loadTags(): void {
    this.quizService.getTags({ pageSize: 100 }).subscribe({
      next: (response) => this.tagOptions.set(response.items),
    });
  }
}
