import { Component, ChangeDetectionStrategy, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AIService } from '../../../core/services/ai.service';
import {
  GenerateQuestionSetRequest,
  GenerateQuestionSetResponse,
  ProviderConfigResponse,
} from '../../../core/models';
import { MessageService } from 'primeng/api';
import { Card } from 'primeng/card';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { InputNumber } from 'primeng/inputnumber';
import { RadioButton } from 'primeng/radiobutton';
import { Chip } from 'primeng/chip';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Message } from 'primeng/message';
import { Slider } from 'primeng/slider';

type Complexity = 'beginner' | 'intermediate' | 'professional' | 'expert';

interface WizardStep {
  label: string;
  icon: string;
}

@Component({
  selector: 'app-generate-wizard',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    Card,
    Button,
    InputText,
    InputNumber,
    RadioButton,
    Chip,
    ProgressSpinner,
    Message,
    Slider,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './generate-wizard.component.html',
})
export class GenerateWizardComponent implements OnInit {
  private readonly aiService = inject(AIService);
  private readonly messageService = inject(MessageService);
  private readonly router = inject(Router);

  // Provider state
  readonly hasProvider = signal<boolean | null>(null);
  readonly checkingProvider = signal(true);

  // Wizard navigation
  readonly currentStep = signal(0);

  // Wizard state
  readonly topics = signal<string[]>([]);
  readonly complexity = signal<Complexity>('beginner');
  readonly experienceYears = signal<number>(1);
  readonly expertiseFieldsText = signal('');
  readonly questionCount = signal(20);
  readonly generating = signal(false);
  readonly result = signal<GenerateQuestionSetResponse | null>(null);

  newTopic = '';

  readonly showExperienceStep = computed(() => {
    const c = this.complexity();
    return c === 'professional' || c === 'expert';
  });

  readonly steps = computed<WizardStep[]>(() => {
    const base: WizardStep[] = [
      { label: 'Topics', icon: 'pi pi-tags' },
      { label: 'Complexity', icon: 'pi pi-sliders-h' },
    ];
    if (this.showExperienceStep()) {
      base.push({ label: 'Experience', icon: 'pi pi-briefcase' });
    }
    base.push({ label: 'Questions', icon: 'pi pi-list' });
    base.push({ label: 'Review', icon: 'pi pi-check' });
    return base;
  });

  readonly complexityOptions: { label: string; value: Complexity }[] = [
    { label: 'Beginner', value: 'beginner' },
    { label: 'Intermediate', value: 'intermediate' },
    { label: 'Professional', value: 'professional' },
    { label: 'Expert', value: 'expert' },
  ];

  readonly canGenerate = computed(() => {
    return this.topics().length > 0 && this.hasProvider() === true && !this.generating();
  });

  /** Maps the current step index to a logical step name */
  readonly currentStepName = computed(() => {
    const idx = this.currentStep();
    const names = ['topics', 'complexity'];
    if (this.showExperienceStep()) names.push('experience');
    names.push('questions', 'review');
    return names[idx] ?? 'topics';
  });

  ngOnInit(): void {
    this.checkProvider();
  }

  nextStep(): void {
    if (this.currentStep() < this.steps().length - 1) {
      this.currentStep.update((s) => s + 1);
    }
  }

  prevStep(): void {
    if (this.currentStep() > 0) {
      this.currentStep.update((s) => s - 1);
    }
  }

  goToStep(index: number): void {
    if (index <= this.currentStep()) {
      this.currentStep.set(index);
    }
  }

  addTopic(): void {
    const topic = this.newTopic.trim();
    if (topic && !this.topics().includes(topic)) {
      this.topics.update((t) => [...t, topic]);
    }
    this.newTopic = '';
  }

  removeTopic(topic: string): void {
    this.topics.update((t) => t.filter((item) => item !== topic));
  }

  generate(): void {
    if (!this.canGenerate()) return;

    this.generating.set(true);
    const request: GenerateQuestionSetRequest = {
      topics: this.topics(),
      complexity: this.complexity(),
      questionCount: this.questionCount(),
    };

    if (this.showExperienceStep()) {
      request.experienceYears = this.experienceYears();
      const fields = this.expertiseFieldsText()
        .split(',')
        .map((f) => f.trim())
        .filter((f) => f.length > 0);
      if (fields.length > 0) {
        request.expertiseFields = fields;
      }
    }

    this.aiService.generateQuestionSet(request).subscribe({
      next: (response) => {
        this.generating.set(false);
        this.result.set(response);
        this.messageService.add({
          severity: 'success',
          summary: 'Generation Complete',
          detail: `Created "${response.title}" with ${response.questionCount} questions.`,
        });
      },
      error: (err) => {
        this.generating.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Generation Failed',
          detail: err?.error?.detail ?? err?.message ?? 'Failed to generate question set.',
        });
      },
    });
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
