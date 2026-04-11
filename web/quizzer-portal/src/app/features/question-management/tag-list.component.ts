import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { QuizService } from '../../core/services/quiz.service';
import { TagResponse } from '../../core/models';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import { Toolbar } from 'primeng/toolbar';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';

@Component({
  selector: 'app-tag-list',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    TableModule,
    Button,
    Dialog,
    InputText,
    Textarea,
    Toolbar,
    ConfirmDialog,
    IconField,
    InputIcon,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <h2>Tags</h2>
    </div>

    <p-toolbar>
      <ng-template #start>
        <p-button label="New Tag" icon="pi pi-plus" (onClick)="openNew()" />
      </ng-template>
      <ng-template #end>
        <p-iconfield>
          <p-inputicon styleClass="pi pi-search" />
          <input pInputText placeholder="Search tags..." [(ngModel)]="searchTerm" (input)="onSearch()" />
        </p-iconfield>
      </ng-template>
    </p-toolbar>

    <p-table
      [value]="tags()"
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
          <th>Description</th>
          <th style="width: 10rem">Actions</th>
        </tr>
      </ng-template>
      <ng-template #body let-tag>
        <tr>
          <td>{{ tag.name }}</td>
          <td>{{ tag.description ?? '\u2014' }}</td>
          <td>
            <p-button icon="pi pi-pencil" [text]="true" [rounded]="true" severity="warn" (onClick)="editTag(tag)" />
            <p-button icon="pi pi-trash" [text]="true" [rounded]="true" severity="danger" (onClick)="confirmDelete(tag)" />
          </td>
        </tr>
      </ng-template>
      <ng-template #emptymessage>
        <tr>
          <td colspan="3" class="text-center p-4">No tags found.</td>
        </tr>
      </ng-template>
    </p-table>

    <p-dialog
      [header]="editingTag() ? 'Edit Tag' : 'New Tag'"
      [(visible)]="displayDialog"
      [modal]="true"
      [style]="{ width: '450px' }"
      [closable]="true"
    >
      <form [formGroup]="tagForm" (ngSubmit)="saveTag()">
        <div class="form-grid">
          <div class="form-field">
            <label for="tagName">Name</label>
            <input id="tagName" pInputText formControlName="name" class="w-full" />
          </div>
          <div class="form-field">
            <label for="tagDesc">Description</label>
            <textarea id="tagDesc" pTextarea formControlName="description" rows="3" class="w-full"></textarea>
          </div>
        </div>
        <div class="dialog-actions">
          <p-button label="Cancel" severity="secondary" [text]="true" (onClick)="displayDialog = false" />
          <p-button label="Save" icon="pi pi-check" type="submit" [disabled]="tagForm.invalid" />
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
export class TagListComponent implements OnInit {
  private readonly quizService = inject(QuizService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly fb = inject(FormBuilder);

  readonly tags = signal<TagResponse[]>([]);
  readonly totalRecords = signal(0);
  readonly loading = signal(true);
  readonly editingTag = signal<TagResponse | null>(null);

  displayDialog = false;
  searchTerm = '';
  pageSize = 10;
  private currentPage = 1;
  private searchTimeout: ReturnType<typeof setTimeout> | null = null;

  tagForm = this.fb.group({
    name: ['', Validators.required],
    description: [''],
  });

  ngOnInit(): void {
    this.loadTags();
  }

  onLazyLoad(event: TableLazyLoadEvent): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? this.pageSize;
    this.currentPage = Math.floor(first / rows) + 1;
    this.pageSize = rows;
    this.loadTags();
  }

  onSearch(): void {
    if (this.searchTimeout) clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.currentPage = 1;
      this.loadTags();
    }, 400);
  }

  openNew(): void {
    this.editingTag.set(null);
    this.tagForm.reset();
    this.displayDialog = true;
  }

  editTag(tag: TagResponse): void {
    this.editingTag.set(tag);
    this.tagForm.patchValue({ name: tag.name, description: tag.description ?? '' });
    this.displayDialog = true;
  }

  saveTag(): void {
    if (this.tagForm.invalid) return;
    const { name, description } = this.tagForm.value;
    const editing = this.editingTag();

    if (editing) {
      this.quizService.updateTag(editing.tagId, {
        tagId: editing.tagId,
        name: name!,
        description: description ?? '',
      }).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Tag updated successfully.' });
          this.displayDialog = false;
          this.loadTags();
        },
        error: () => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update tag.' });
        },
      });
    } else {
      this.quizService.createTag({
        name: name!,
        description: description || null,
      }).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Tag created successfully.' });
          this.displayDialog = false;
          this.loadTags();
        },
        error: () => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to create tag.' });
        },
      });
    }
  }

  confirmDelete(tag: TagResponse): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete tag "${tag.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.quizService.deleteTag(tag.tagId).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Deleted', detail: 'Tag deleted successfully.' });
            this.loadTags();
          },
          error: () => {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete tag.' });
          },
        });
      },
    });
  }

  private loadTags(): void {
    this.loading.set(true);
    this.quizService.getTags({
      searchName: this.searchTerm || undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
    }).subscribe({
      next: (response) => {
        this.tags.set(response.items);
        this.totalRecords.set(response.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }
}
