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
  template: `
    <div class="page-header">
      <h2>Question Sets</h2>
    </div>

    <p-toolbar>
      <ng-template #start>
        <p-button label="New Question Set" icon="pi pi-plus" (onClick)="openNew()" />
      </ng-template>
      <ng-template #end>
        <div class="toolbar-filters">
          <p-select
            [options]="tagOptions()"
            [(ngModel)]="selectedTagId"
            optionLabel="name"
            optionValue="tagId"
            placeholder="Filter by tag"
            [showClear]="true"
            (onChange)="onTagFilterChange()"
            [style]="{ minWidth: '200px' }"
          />
          <p-iconfield>
            <p-inputicon styleClass="pi pi-search" />
            <input pInputText placeholder="Search by name..." [(ngModel)]="searchTerm" (input)="onSearch()" />
          </p-iconfield>
        </div>
      </ng-template>
    </p-toolbar>

    <p-table
      [value]="questionSets()"
      [lazy]="true"
      [paginator]="true"
      [rows]="pageSize"
      [totalRecords]="totalRecords()"
      [loading]="loading()"
      (onLazyLoad)="onLazyLoad($event)"
      [rowHover]="true"
      styleClass="p-datatable-sm"
    >
      <ng-template #header>
        <tr>
          <th>Name</th>
          <th>Set Code</th>
          <th>Details</th>
          <th>Questions</th>
          <th style="width: 12rem">Actions</th>
        </tr>
      </ng-template>
      <ng-template #body let-qs>
        <tr>
          <td>{{ qs.name }}</td>
          <td>{{ qs.setCode ?? '\u2014' }}</td>
          <td>{{ truncate(qs.details, 60) }}</td>
          <td>{{ qs.questions?.length ?? 0 }}</td>
          <td>
            <p-button icon="pi pi-eye" [text]="true" [rounded]="true" severity="info" (onClick)="viewSet(qs)" />
            <p-button icon="pi pi-pencil" [text]="true" [rounded]="true" severity="warn" (onClick)="editSet(qs)" />
            <p-button icon="pi pi-trash" [text]="true" [rounded]="true" severity="danger" (onClick)="confirmDelete(qs)" />
          </td>
        </tr>
      </ng-template>
      <ng-template #emptymessage>
        <tr>
          <td colspan="5" class="text-center p-4">No question sets found.</td>
        </tr>
      </ng-template>
    </p-table>

    <p-dialog
      [header]="editingSet() ? 'Edit Question Set' : 'New Question Set'"
      [(visible)]="displayDialog"
      [modal]="true"
      [style]="{ width: '500px' }"
      [closable]="true"
    >
      <form [formGroup]="setForm" (ngSubmit)="saveSet()">
        <div class="form-grid">
          <div class="form-field">
            <label for="setName">Name</label>
            <input id="setName" pInputText formControlName="name" class="w-full" />
          </div>
          <div class="form-field">
            <label for="setCode">Set Code</label>
            <input id="setCode" pInputText formControlName="setCode" class="w-full" />
          </div>
          <div class="form-field">
            <label for="setDetails">Details</label>
            <textarea id="setDetails" pTextarea formControlName="details" rows="3" class="w-full"></textarea>
          </div>
        </div>
        <div class="dialog-actions">
          <p-button label="Cancel" severity="secondary" [text]="true" (onClick)="displayDialog = false" />
          <p-button label="Save" icon="pi pi-check" type="submit" [disabled]="setForm.invalid" />
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
  `],
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
