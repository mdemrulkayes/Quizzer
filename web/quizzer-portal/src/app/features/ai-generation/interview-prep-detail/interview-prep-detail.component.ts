import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AIService } from '../../../core/services/ai.service';
import { InterviewPrepMaterialDetail } from '../../../core/models';
import { Tag } from 'primeng/tag';
import { Accordion, AccordionContent, AccordionHeader, AccordionPanel } from 'primeng/accordion';
import { Card } from 'primeng/card';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Button } from 'primeng/button';

@Component({
  selector: 'app-interview-prep-detail',
  standalone: true,
  imports: [DatePipe, RouterLink, Tag, Accordion, AccordionPanel, AccordionHeader, AccordionContent, Card, ProgressSpinner, Button],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './interview-prep-detail.component.html',
})
export class InterviewPrepDetailComponent implements OnInit {
  private readonly aiService = inject(AIService);
  private readonly route = inject(ActivatedRoute);

  readonly material = signal<InterviewPrepMaterialDetail | null>(null);
  readonly loading = signal(true);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadMaterial(id);
    }
  }

  private loadMaterial(id: string): void {
    this.loading.set(true);
    this.aiService.getInterviewPrepMaterial(id).subscribe({
      next: (detail) => {
        this.material.set(detail);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }
}
