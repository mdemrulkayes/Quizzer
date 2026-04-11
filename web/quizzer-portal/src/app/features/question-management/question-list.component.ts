import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-question-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<h2>Questions</h2><p>Coming soon...</p>`,
})
export class QuestionListComponent {}
