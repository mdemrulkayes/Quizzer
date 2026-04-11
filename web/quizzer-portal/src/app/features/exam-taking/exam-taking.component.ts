import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-exam-taking',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<h2>Exam</h2><p>Coming soon...</p>`,
})
export class ExamTakingComponent {}
