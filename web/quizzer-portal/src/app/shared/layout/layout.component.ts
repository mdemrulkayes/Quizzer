import { Component, ChangeDetectionStrategy, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from './sidebar/sidebar.component';
import { TopbarComponent } from './topbar/topbar.component';
import { Toast } from 'primeng/toast';
import { ConfirmDialog } from 'primeng/confirmdialog';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, TopbarComponent, Toast, ConfirmDialog],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="app-layout">
      <app-sidebar [collapsed]="sidebarCollapsed()" />
      <div class="app-main">
        <app-topbar (toggleSidebar)="toggleSidebar()" />
        <div class="app-content">
          <router-outlet />
        </div>
      </div>
    </div>
    <p-toast />
    <p-confirmdialog />
  `,
  styles: [`
    .app-layout {
      display: flex;
      min-height: 100vh;
    }

    .app-main {
      flex: 1;
      display: flex;
      flex-direction: column;
      min-width: 0;
    }

    .app-content {
      flex: 1;
      padding: 1.5rem;
      background: var(--p-surface-50);
      overflow-y: auto;
    }
  `],
})
export class LayoutComponent {
  readonly sidebarCollapsed = signal(false);

  toggleSidebar(): void {
    this.sidebarCollapsed.update((v) => !v);
  }
}
