import { Component, ChangeDetectionStrategy, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AIService } from '../../../core/services/ai.service';
import {
  GenerateFromJobDescriptionRequest,
  GenerateFromJobDescriptionResponse,
  ProviderConfigResponse,
} from '../../../core/models';
import { MessageService } from 'primeng/api';
import { Card } from 'primeng/card';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { InputNumber } from 'primeng/inputnumber';
import { Textarea } from 'primeng/textarea';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Message } from 'primeng/message';

type OutputType = 'question_set' | 'interview_prep';

@Component({
  selector: 'app-job-description',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    Card,
    Button,
    InputText,
    InputNumber,
    Textarea,
    ProgressSpinner,
    Message,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './job-description.component.html',
})
export class JobDescriptionComponent implements OnInit {
  private readonly aiService = inject(AIService);
  private readonly messageService = inject(MessageService);

  // Provider state
  readonly hasProvider = signal<boolean | null>(null);
  readonly checkingProvider = signal(true);

  // Form state
  readonly jobTitle = signal('');
  readonly jobDescription = signal('');
  readonly outputType = signal<OutputType>('question_set');
  readonly questionCount = signal(20);
  readonly generating = signal(false);
  readonly result = signal<GenerateFromJobDescriptionResponse | null>(null);

  readonly canGenerate = computed(() => {
    return (
      this.jobTitle().trim().length > 0 &&
      this.jobDescription().trim().length > 0 &&
      this.hasProvider() === true &&
      !this.generating()
    );
  });

  readonly resultLink = computed(() => {
    const r = this.result();
    if (!r) return '/dashboard';
    return r.outputType === 'question_set' ? '/question-sets' : '/ai/interview-prep';
  });

  readonly resultLinkLabel = computed(() => {
    const r = this.result();
    if (!r) return '';
    return r.outputType === 'question_set' ? 'View Question Sets' : 'View Interview Prep Materials';
  });

  ngOnInit(): void {
    this.checkProvider();
  }

  selectOutputType(type: OutputType): void {
    this.outputType.set(type);
  }

  generate(): void {
    if (!this.canGenerate()) return;

    this.generating.set(true);
    const request: GenerateFromJobDescriptionRequest = {
      jobTitle: this.jobTitle(),
      jobDescription: this.jobDescription(),
      outputType: this.outputType(),
      questionCount: this.outputType() === 'question_set' ? this.questionCount() : 0,
    };

    this.aiService.generateFromJobDescription(request).subscribe({
      next: (response) => {
        this.generating.set(false);
        this.result.set(response);
        this.messageService.add({
          severity: 'success',
          summary: 'Generation Complete',
          detail: `Created "${response.title}" successfully.`,
        });
      },
      error: (err) => {
        this.generating.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Generation Failed',
          detail: err?.error?.detail ?? err?.message ?? 'Failed to generate from job description.',
        });
      },
    });
  }

  reset(): void {
    this.result.set(null);
    this.jobTitle.set('');
    this.jobDescription.set('');
    this.outputType.set('question_set');
    this.questionCount.set(20);
  }

  private checkProvider(): void {
    this.checkingProvider.set(true);
    this.aiService.getProviderConfig().subscribe({
      next: (config: ProviderConfigResponse) => {
        this.hasProvider.set(config.isActive);
        this.checkingProvider.set(false);
      },
      error: () => {
        this.hasProvider.set(false);
        this.checkingProvider.set(false);
      },
    });
  }
}
