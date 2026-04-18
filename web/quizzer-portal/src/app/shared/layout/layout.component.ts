import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { Router, NavigationStart, NavigationEnd, NavigationCancel, NavigationError } from '@angular/router';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from './sidebar/sidebar.component';
import { TopbarComponent } from './topbar/topbar.component';
import { Toast } from 'primeng/toast';
import { ProgressBar } from 'primeng/progressbar';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, TopbarComponent, Toast, ProgressBar],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss',
})
export class LayoutComponent {
  private readonly router = inject(Router);
  readonly sidebarCollapsed = signal(false);
  readonly navigating = signal(false);

  constructor() {
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationStart) {
        this.navigating.set(true);
      } else if (
        event instanceof NavigationEnd ||
        event instanceof NavigationCancel ||
        event instanceof NavigationError
      ) {
        this.navigating.set(false);
      }
    });
  }

  toggleSidebar(): void {
    this.sidebarCollapsed.update((v) => !v);
  }
}
