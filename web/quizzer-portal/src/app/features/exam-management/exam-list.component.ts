import { Component, ChangeDetectionStrategy, inject, signal, computed, OnInit } from '@angular/core';
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
  templateUrl: './exam-list.component.html',
  styleUrl: './exam-list.component.scss',
})
export class ExamListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly examService = inject(ExamService);
  private readonly quizService = inject(QuizService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly router = inject(Router);

  readonly exams = signal<ExamResponse[]>([]);
  readonly totalRecords = signal(0);
  readonly pageSize = signal(10);
  readonly loading = signal(true);
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

  ngOnInit(): void {
    this.loadQuestionSets();
  }

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
