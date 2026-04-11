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
  templateUrl: './tag-list.component.html',
  styleUrl: './tag-list.component.scss',
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
