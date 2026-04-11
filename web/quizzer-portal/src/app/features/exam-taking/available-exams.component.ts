import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-available-exams',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<h2>Available Exams</h2><p>Coming soon...</p>`,
})
export class AvailableExamsComponent {}
