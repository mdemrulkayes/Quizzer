import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { AIService } from '../../../core/services/ai.service';
import { InterviewPrepMaterialSummary } from '../../../core/models';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { Button } from 'primeng/button';

@Component({
  selector: 'app-interview-prep-list',
  standalone: true,
  imports: [DatePipe, TableModule, Tag, Button],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './interview-prep-list.component.html',
})
export class InterviewPrepListComponent implements OnInit {
  private readonly aiService = inject(AIService);
  private readonly router = inject(Router);

  readonly materials = signal<InterviewPrepMaterialSummary[]>([]);
  readonly loading = signal(true);

  pageSize = 10;

  ngOnInit(): void {
    this.loadMaterials();
  }

  viewDetail(item: InterviewPrepMaterialSummary): void {
    this.router.navigate(['/ai/interview-prep', item.id]);
  }

  private loadMaterials(): void {
    this.loading.set(true);
    this.aiService.getInterviewPrepMaterials({ pageSize: 100 }).subscribe({
      next: (items) => {
        this.materials.set(items);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }
}
