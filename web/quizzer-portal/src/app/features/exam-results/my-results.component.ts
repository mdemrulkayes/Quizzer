import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-my-results',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<h2>My Results</h2><p>Coming soon...</p>`,
})
export class MyResultsComponent {}
