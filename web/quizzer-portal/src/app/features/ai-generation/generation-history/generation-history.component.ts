import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { AIService } from '../../../core/services/ai.service';
import { GenerationHistoryItem } from '../../../core/models';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';

@Component({
  selector: 'app-generation-history',
  standalone: true,
  imports: [DatePipe, TableModule, Tag, Tooltip],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './generation-history.component.html',
})
export class GenerationHistoryComponent implements OnInit {
  private readonly aiService = inject(AIService);

  readonly history = signal<GenerationHistoryItem[]>([]);
  readonly loading = signal(true);

  pageSize = 10;

  ngOnInit(): void {
    this.loadHistory();
  }

  getSourceLabel(source: string): string {
    switch (source) {
      case 'topic': return 'Topic Based';
      case 'job_description': return 'Job Description';
      default: return source;
    }
  }

  getOutputTypeLabel(type: string): string {
    switch (type) {
      case 'question_set': return 'Question Set';
      case 'interview_prep': return 'Interview Prep';
      default: return type;
    }
  }

  getStatusSeverity(status: string): 'success' | 'danger' | 'warn' | 'info' {
    switch (status.toLowerCase()) {
      case 'completed': return 'success';
      case 'failed': return 'danger';
      case 'pending': return 'warn';
      default: return 'info';
    }
  }

  private loadHistory(): void {
    this.loading.set(true);
    this.aiService.getGenerationHistory({ pageSize: 100 }).subscribe({
      next: (items) => {
        this.history.set(items.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()));
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }
}
