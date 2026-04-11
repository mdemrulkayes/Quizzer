import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-tag-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<h2>Tags</h2><p>Coming soon...</p>`,
})
export class TagListComponent {}
