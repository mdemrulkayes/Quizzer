import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink, Button],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="not-found">
      <h1>404</h1>
      <p>Page not found</p>
      <p-button label="Go to Dashboard" icon="pi pi-home" routerLink="/dashboard" />
    </div>
  `,
  styles: [`
    .not-found {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      text-align: center;
    }
    h1 { font-size: 6rem; margin: 0; color: var(--p-primary-color); }
    p { font-size: 1.25rem; color: var(--p-text-muted-color); margin-bottom: 2rem; }
  `],
})
export class NotFoundComponent {}
