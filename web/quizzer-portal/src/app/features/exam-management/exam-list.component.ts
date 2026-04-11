import { Component, ChangeDetectionStrategy, inject, signal, computed } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { SlicePipe } from '@angular/common';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import { InputNumber } from 'primeng/inputnumber';
import { DatePicker } from 'primeng/datepicker';
import { Select } from 'primeng/select';
import { Tag } from 'primeng/tag';
import { Toolbar } from 'primeng/toolbar';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { ExamService } from '../../core/services/exam.service';
import { QuizService } from '../../core/services/quiz.service';
import {
  ExamResponse,
  CreateExamRequest,
  UpdateExamRequest,
  QuestionSetResponse,
} from '../../core/models';

@Component({
  selector: 'app-exam-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    SlicePipe,
    TableModule,
    Button,
    Dialog,
    InputText,
    Textarea,
    InputNumber,
    DatePicker,
    Select,
    Tag,
    Toolbar,
    ConfirmDialog,
  ],
  providers: [ConfirmationService],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <p-toolbar>
      <ng-template #start>
        <h2 style="margin: 0;">Exam Management</h2>
      </ng-template>
      <ng-template #end>
        <p-button label="Create Exam" icon="pi pi-plus" (onClick)="openCreateDialog()" />
      </ng-template>
    </p-toolbar>

    <p-table
      [value]="exams()"
      [lazy]="true"
      [paginator]="true"
      [rows]="pageSize()"
      [totalRecords]="totalRecords()"
      [loading]="loading()"
      [rowsPerPageOptions]="[5, 10, 25]"
      (onLazyLoad)="onLazyLoad($event)"
    >
      <ng-template #header>
        <tr>
          <th>Title</th>
          <th>Description</th>
          <th>Duration (min)</th>
          <th>Total Marks</th>
          <th>Passing Marks</th>
          <th>Status</th>
          <th>Actions</th>
        </tr>
      </ng-template>
      <ng-template #body let-exam>
        <tr>
          <td>{{ exam.title }}</td>
          <td>{{ exam.description | slice:0:50 }}@if (exam.description && exam.description.length > 50) {…}</td>
          <td>{{ exam.durationInMinutes }}</td>
          <td>{{ exam.totalMarks }}</td>
          <td>{{ exam.passingMarks }}</td>
          <td>
            <p-tag
              [value]="exam.isPublished ? 'Published' : 'Draft'"
              [severity]="exam.isPublished ? 'success' : 'warn'"
            />
          </td>
          <td class="action-buttons">
            @if (!exam.isPublished) {
              <p-button icon="pi pi-pencil" [rounded]="true" [text]="true" severity="info" (onClick)="openEditDialog(exam)" pTooltip="Edit" />
            }
            @if (exam.isPublished) {
              <p-button icon="pi pi-times" [rounded]="true" [text]="true" severity="warn" (onClick)="confirmTogglePublish(exam)" pTooltip="Unpublish" />
            } @else {
              <p-button icon="pi pi-check" [rounded]="true" [text]="true" severity="success" (onClick)="confirmTogglePublish(exam)" pTooltip="Publish" />
            }
            <p-button icon="pi pi-chart-bar" [rounded]="true" [text]="true" severity="secondary" (onClick)="viewResults(exam)" pTooltip="View Results" />
            @if (!exam.isPublished) {
              <p-button icon="pi pi-trash" [rounded]="true" [text]="true" severity="danger" (onClick)="confirmDelete(exam)" pTooltip="Delete" />
            }
          </td>
        </tr>
      </ng-template>
      <ng-template #emptymessage>
        <tr>
          <td colspan="7" class="text-center">No exams found.</td>
        </tr>
      </ng-template>
    </p-table>

    <p-dialog
      [header]="editingExam() ? 'Edit Exam' : 'Create Exam'"
      [(visible)]="dialogVisible"
      [modal]="true"
      [style]="{ width: '36rem' }"
      (onHide)="onDialogHide()"
    >
      <form [formGroup]="form" (ngSubmit)="onSave()">
        <div class="form-grid">
          <div class="form-field">
            <label for="title">Title *</label>
            <input pInputText id="title" formControlName="title" class="w-full" />
          </div>
          <div class="form-field">
            <label for="description">Description</label>
            <textarea pTextarea id="description" formControlName="description" class="w-full" rows="3"></textarea>
          </div>
          @if (!editingExam()) {
            <div class="form-field">
              <label for="questionSetId">Question Set *</label>
              <p-select
                id="questionSetId"
                formControlName="questionSetId"
                [options]="questionSets()"
                optionLabel="name"
                optionValue="questionSetId"
                placeholder="Select a question set"
                class="w-full"
              />
            </div>
          }
          <div class="form-row">
            <div class="form-field">
              <label for="durationInMinutes">Duration (min)</label>
              <p-inputnumber id="durationInMinutes" formControlName="durationInMinutes" class="w-full" />
            </div>
            <div class="form-field">
              <label for="totalMarks">Total Marks</label>
              <p-inputnumber id="totalMarks" formControlName="totalMarks" class="w-full" />
            </div>
            <div class="form-field">
              <label for="passingMarks">Passing Marks</label>
              <p-inputnumber id="passingMarks" formControlName="passingMarks" class="w-full" />
            </div>
          </div>
          <div class="form-row">
            <div class="form-field">
              <label for="scheduledStartTime">Scheduled Start</label>
              <p-datepicker id="scheduledStartTime" formControlName="scheduledStartTime" [showTime]="true" class="w-full" />
            </div>
            <div class="form-field">
              <label for="scheduledEndTime">Scheduled End</label>
              <p-datepicker id="scheduledEndTime" formControlName="scheduledEndTime" [showTime]="true" class="w-full" />
            </div>
          </div>
          <div class="form-actions">
            <p-button label="Cancel" severity="secondary" (onClick)="dialogVisible = false" />
            <p-button
              label="Save"
              icon="pi pi-check"
              type="submit"
              [disabled]="form.invalid"
              [loading]="saving()"
            />
          </div>
        </div>
      </form>
    </p-dialog>

    <p-confirmDialog />
  `,
  styles: [`
    :host {
      display: block;
    }

    .action-buttons {
      display: flex;
      gap: 0.25rem;
    }

    .form-grid {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .form-field {
      display: flex;
      flex-direction: column;
      gap: 0.375rem;
    }

    .form-field label {
      font-weight: 600;
      font-size: 0.875rem;
    }

    .form-row {
      display: flex;
      gap: 1rem;
    }

    .form-row .form-field {
      flex: 1;
    }

    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.5rem;
      margin-top: 0.5rem;
    }

    .text-center {
      text-align: center;
    }
  `],
})
export class ExamListComponent {
  private readonly fb = inject(FormBuilder);
  private readonly examService = inject(ExamService);
  private readonly quizService = inject(QuizService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly router = inject(Router);

  readonly exams = signal<ExamResponse[]>([]);
  readonly totalRecords = signal(0);
  readonly pageSize = signal(10);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly questionSets = signal<QuestionSetResponse[]>([]);
  readonly editingExam = signal<ExamResponse | null>(null);
  dialogVisible = false;

  readonly isEditing = computed(() => !!this.editingExam());

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required]],
    description: [''],
    questionSetId: [null as number | null, [Validators.required]],
    durationInMinutes: [60],
    totalMarks: [100],
    passingMarks: [40],
    scheduledStartTime: [null as Date | null],
    scheduledEndTime: [null as Date | null],
  });

  private currentPage = 1;

  loadExams(pageNumber = 1, pageSize = 10): void {
    this.loading.set(true);
    this.currentPage = pageNumber;
    this.examService.getExams(pageNumber, pageSize).subscribe({
      next: (result) => {
        this.exams.set(result.items);
        this.totalRecords.set(result.totalCount);
        this.pageSize.set(result.pageSize);
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load exams.' });
        this.loading.set(false);
      },
    });
  }

  onLazyLoad(event: TableLazyLoadEvent): void {
    const rows = event.rows ?? 10;
    const first = event.first ?? 0;
    const pageNumber = Math.floor(first / rows) + 1;
    this.loadExams(pageNumber, rows);
  }

  openCreateDialog(): void {
    this.editingExam.set(null);
    this.form.reset({
      title: '',
      description: '',
      questionSetId: null,
      durationInMinutes: 60,
      totalMarks: 100,
      passingMarks: 40,
      scheduledStartTime: null,
      scheduledEndTime: null,
    });
    this.form.controls.questionSetId.setValidators([Validators.required]);
    this.form.controls.questionSetId.updateValueAndValidity();
    this.loadQuestionSets();
    this.dialogVisible = true;
  }

  openEditDialog(exam: ExamResponse): void {
    this.editingExam.set(exam);
    this.form.reset({
      title: exam.title,
      description: exam.description ?? '',
      questionSetId: exam.questionSetId,
      durationInMinutes: exam.durationInMinutes,
      totalMarks: exam.totalMarks,
      passingMarks: exam.passingMarks,
      scheduledStartTime: exam.scheduledStartTime ? new Date(exam.scheduledStartTime) : null,
      scheduledEndTime: exam.scheduledEndTime ? new Date(exam.scheduledEndTime) : null,
    });
    this.form.controls.questionSetId.clearValidators();
    this.form.controls.questionSetId.updateValueAndValidity();
    this.dialogVisible = true;
  }

  onDialogHide(): void {
    this.editingExam.set(null);
  }

  onSave(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    const val = this.form.getRawValue();
    const editing = this.editingExam();

    if (editing) {
      const request: UpdateExamRequest = {
        examId: editing.examId,
        title: val.title,
        description: val.description || null,
        durationInMinutes: val.durationInMinutes,
        totalMarks: val.totalMarks,
        passingMarks: val.passingMarks,
        scheduledStartTime: val.scheduledStartTime ? val.scheduledStartTime.toISOString() : null,
        scheduledEndTime: val.scheduledEndTime ? val.scheduledEndTime.toISOString() : null,
      };
      this.examService.updateExam(editing.examId, request).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Exam updated successfully.' });
          this.saving.set(false);
          this.dialogVisible = false;
          this.loadExams(this.currentPage, this.pageSize());
        },
        error: () => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update exam.' });
          this.saving.set(false);
        },
      });
    } else {
      const request: CreateExamRequest = {
        title: val.title,
        description: val.description || null,
        questionSetId: val.questionSetId!,
        durationInMinutes: val.durationInMinutes,
        totalMarks: val.totalMarks,
        passingMarks: val.passingMarks,
        scheduledStartTime: val.scheduledStartTime ? val.scheduledStartTime.toISOString() : null,
        scheduledEndTime: val.scheduledEndTime ? val.scheduledEndTime.toISOString() : null,
      };
      this.examService.createExam(request).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Exam created successfully.' });
          this.saving.set(false);
          this.dialogVisible = false;
          this.loadExams(this.currentPage, this.pageSize());
        },
        error: () => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to create exam.' });
          this.saving.set(false);
        },
      });
    }
  }

  confirmTogglePublish(exam: ExamResponse): void {
    const action = exam.isPublished ? 'unpublish' : 'publish';
    this.confirmationService.confirm({
      message: `Are you sure you want to ${action} "${exam.title}"?`,
      header: `Confirm ${action.charAt(0).toUpperCase() + action.slice(1)}`,
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        const obs = exam.isPublished
          ? this.examService.unpublishExam(exam.examId)
          : this.examService.publishExam(exam.examId);
        obs.subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Success', detail: `Exam ${action}ed successfully.` });
            this.loadExams(this.currentPage, this.pageSize());
          },
          error: () => {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: `Failed to ${action} exam.` });
          },
        });
      },
    });
  }

  confirmDelete(exam: ExamResponse): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete "${exam.title}"? This action cannot be undone.`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.examService.deleteExam(exam.examId).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Exam deleted successfully.' });
            this.loadExams(this.currentPage, this.pageSize());
          },
          error: () => {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete exam.' });
          },
        });
      },
    });
  }

  viewResults(exam: ExamResponse): void {
    this.router.navigate(['/exam', exam.examId, 'results']);
  }

  private loadQuestionSets(): void {
    this.quizService.getQuestionSets({ pageSize: 100 }).subscribe({
      next: (result) => this.questionSets.set(result.items),
      error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load question sets.' }),
    });
  }
}
