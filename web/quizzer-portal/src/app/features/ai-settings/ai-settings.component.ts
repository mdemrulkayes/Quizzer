import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { AIService } from '../../core/services/ai.service';
import { SupportedProvider, ProviderConfigResponse } from '../../core/models';
import { MessageService, ConfirmationService } from 'primeng/api';
import { Card } from 'primeng/card';
import { Select } from 'primeng/select';
import { Button } from 'primeng/button';
import { Tag } from 'primeng/tag';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { Password } from 'primeng/password';

@Component({
  selector: 'app-ai-settings',
  standalone: true,
  imports: [
    FormsModule,
    DatePipe,
    Card,
    Select,
    Button,
    Tag,
    ConfirmDialog,
    Password,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './ai-settings.component.html',
})
export class AISettingsComponent implements OnInit {
  private readonly aiService = inject(AIService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);

  readonly providers = signal<SupportedProvider[]>([]);
  readonly currentConfig = signal<ProviderConfigResponse | null>(null);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly testing = signal(false);

  selectedProviderId = '';
  apiKey = '';

  ngOnInit(): void {
    this.loadProviders();
    this.loadCurrentConfig();
  }

  saveConfig(): void {
    if (!this.selectedProviderId || !this.apiKey) return;
    this.saving.set(true);
    this.aiService.saveProviderConfig({ providerId: this.selectedProviderId, secretKey: this.apiKey }).subscribe({
      next: (config) => {
        this.currentConfig.set(config);
        this.apiKey = '';
        this.saving.set(false);
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Provider configuration saved.' });
      },
      error: () => {
        this.saving.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to save provider configuration.' });
      },
    });
  }

  testConnection(): void {
    this.testing.set(true);
    this.aiService.testProviderConnection().subscribe({
      next: (result) => {
        this.testing.set(false);
        this.loadCurrentConfig();
        if (result.success) {
          this.messageService.add({ severity: 'success', summary: 'Connection Successful', detail: result.message ?? 'Provider is reachable.' });
        } else {
          this.messageService.add({ severity: 'warn', summary: 'Connection Failed', detail: result.message ?? 'Could not reach the provider.' });
        }
      },
      error: () => {
        this.testing.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to test connection.' });
      },
    });
  }

  confirmDelete(): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to delete the provider configuration? This will disable AI features.',
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.aiService.deleteProviderConfig().subscribe({
          next: () => {
            this.currentConfig.set(null);
            this.selectedProviderId = '';
            this.apiKey = '';
            this.messageService.add({ severity: 'success', summary: 'Deleted', detail: 'Provider configuration deleted.' });
          },
          error: () => {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete provider configuration.' });
          },
        });
      },
    });
  }

  getTestResultSeverity(): 'success' | 'danger' | 'info' {
    const result = this.currentConfig()?.lastTestResult;
    if (result === 'success') return 'success';
    if (result === 'failed') return 'danger';
    return 'info';
  }

  private loadProviders(): void {
    this.loading.set(true);
    this.aiService.getSupportedProviders().subscribe({
      next: (providers) => {
        this.providers.set(providers);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }

  private loadCurrentConfig(): void {
    this.aiService.getProviderConfig().subscribe({
      next: (config) => {
        this.currentConfig.set(config);
        this.selectedProviderId = config.providerId;
      },
      error: () => {
        this.currentConfig.set(null);
      },
    });
  }
}
