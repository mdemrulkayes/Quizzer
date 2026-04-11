import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-user-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<h2>User Management</h2><p>Coming soon...</p>`,
})
export class UserListComponent {}
