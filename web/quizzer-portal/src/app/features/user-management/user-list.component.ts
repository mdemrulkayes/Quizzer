import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { IdentityService } from '../../core/services/identity.service';
import { UserListItem, UserRole } from '../../core/models';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { Button } from 'primeng/button';
import { Tag } from 'primeng/tag';
import { Dialog } from 'primeng/dialog';
import { Select } from 'primeng/select';
import { InputText } from 'primeng/inputtext';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    TableModule,
    Button,
    Tag,
    Dialog,
    Select,
    InputText,
    ConfirmDialog,
    IconField,
    InputIcon,
  ],
  providers: [ConfirmationService, MessageService],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <h2>User Management</h2>
      <p-iconfield>
        <p-inputicon styleClass="pi pi-search" />
        <input pInputText placeholder="Search users..." [(ngModel)]="searchTerm" (input)="onSearch()" />
      </p-iconfield>
    </div>

    <p-table
      [value]="users()"
      [lazy]="true"
      [paginator]="true"
      [rows]="pageSize"
      [totalRecords]="totalRecords()"
      [loading]="loading()"
      (onLazyLoad)="onLazyLoad($event)"
      [rowHover]="true"
      styleClass="p-datatable-sm"
    >
      <ng-template #header>
        <tr>
          <th>Name</th>
          <th>Email</th>
          <th>Roles</th>
          <th>Created</th>
          <th>Last Login</th>
          <th style="width: 12rem">Actions</th>
        </tr>
      </ng-template>
      <ng-template #body let-user>
        <tr>
          <td>{{ user.firstName }} {{ user.lastName }}</td>
          <td>{{ user.email }}</td>
          <td>
            @for (role of user.roles; track role) {
              <p-tag [value]="role" [severity]="getRoleSeverity(role)" styleClass="mr-1" />
            }
          </td>
          <td>{{ user.createdDate | date:'mediumDate' }}</td>
          <td>{{ user.lastLoginTime ? (user.lastLoginTime | date:'medium') : 'Never' }}</td>
          <td>
            <p-button icon="pi pi-eye" [text]="true" [rounded]="true" severity="info" (onClick)="viewUser(user)" />
            @if (authService.isSuperAdmin) {
              <p-button icon="pi pi-pencil" [text]="true" [rounded]="true" severity="warn" (onClick)="editRole(user)" />
            }
            <p-button icon="pi pi-trash" [text]="true" [rounded]="true" severity="danger" (onClick)="confirmDelete(user)" />
          </td>
        </tr>
      </ng-template>
      <ng-template #emptymessage>
        <tr>
          <td colspan="6" class="text-center p-4">No users found.</td>
        </tr>
      </ng-template>
    </p-table>

    <!-- User Detail Dialog -->
    <p-dialog
      header="User Details"
      [(visible)]="displayDetailDialog"
      [modal]="true"
      [style]="{ width: '480px' }"
      [closable]="true"
    >
      @if (selectedUser()) {
        <div class="detail-grid">
          <div class="detail-row">
            <span class="detail-label">Name</span>
            <span class="detail-value">{{ selectedUser()!.firstName }} {{ selectedUser()!.lastName }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Email</span>
            <span class="detail-value">{{ selectedUser()!.email ?? '—' }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Phone</span>
            <span class="detail-value">{{ selectedUser()!.phoneNumber ?? '—' }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Roles</span>
            <span class="detail-value">
              @for (role of selectedUser()!.roles; track role) {
                <p-tag [value]="role" [severity]="getRoleSeverity(role)" styleClass="mr-1" />
              }
            </span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Status</span>
            <span class="detail-value">
              <p-tag [value]="selectedUser()!.isDeleted ? 'Deleted' : 'Active'" [severity]="selectedUser()!.isDeleted ? 'danger' : 'success'" />
            </span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Created</span>
            <span class="detail-value">{{ selectedUser()!.createdDate | date:'medium' }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Last Login</span>
            <span class="detail-value">{{ selectedUser()!.lastLoginTime ? (selectedUser()!.lastLoginTime | date:'medium') : 'Never' }}</span>
          </div>
        </div>
      }
    </p-dialog>

    <!-- Role Edit Dialog -->
    <p-dialog
      header="Change User Role"
      [(visible)]="displayRoleDialog"
      [modal]="true"
      [style]="{ width: '400px' }"
      [closable]="true"
    >
      @if (selectedUser()) {
        <div class="role-edit-content">
          <p>Changing role for <strong>{{ selectedUser()!.firstName }} {{ selectedUser()!.lastName }}</strong></p>
          <p-select
            [options]="roleOptions"
            [(ngModel)]="selectedRole"
            optionLabel="label"
            optionValue="value"
            placeholder="Select a role"
            [style]="{ width: '100%' }"
          />
          <div class="role-edit-actions">
            <p-button label="Cancel" severity="secondary" [text]="true" (onClick)="displayRoleDialog = false" />
            <p-button label="Update Role" severity="warn" (onClick)="updateRole()" [disabled]="!selectedRole" />
          </div>
        </div>
      }
    </p-dialog>

    <p-confirmDialog />
  `,
  styles: [`
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
      flex-wrap: wrap;
      gap: 1rem;
    }

    .page-header h2 {
      margin: 0;
      color: var(--p-text-color);
      font-size: 1.5rem;
    }

    .detail-grid {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .detail-row {
      display: flex;
      gap: 1rem;
      align-items: flex-start;
    }

    .detail-label {
      font-weight: 600;
      color: var(--p-text-muted-color);
      min-width: 90px;
      flex-shrink: 0;
    }

    .detail-value {
      color: var(--p-text-color);
    }

    .role-edit-content {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .role-edit-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.5rem;
      margin-top: 0.5rem;
    }

    .text-center {
      text-align: center;
    }

    :host ::ng-deep .mr-1 {
      margin-right: 0.25rem;
    }
  `],
})
export class UserListComponent implements OnInit {
  readonly authService = inject(AuthService);
  private readonly identityService = inject(IdentityService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);

  readonly users = signal<UserListItem[]>([]);
  readonly totalRecords = signal(0);
  readonly loading = signal(true);
  readonly selectedUser = signal<UserListItem | null>(null);

  displayDetailDialog = false;
  displayRoleDialog = false;
  searchTerm = '';
  selectedRole = '';
  pageSize = 10;

  private currentPage = 1;
  private searchTimeout: ReturnType<typeof setTimeout> | null = null;

  readonly roleOptions = [
    { label: 'Super Admin', value: UserRole.SuperAdmin },
    { label: 'Support Admin', value: UserRole.SupportAdmin },
    { label: 'Quiz Author', value: UserRole.QuizAuthor },
    { label: 'Examinee', value: UserRole.Examine },
  ];

  ngOnInit(): void {
    this.loadUsers();
  }

  onLazyLoad(event: TableLazyLoadEvent): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? this.pageSize;
    this.currentPage = Math.floor(first / rows) + 1;
    this.pageSize = rows;
    this.loadUsers();
  }

  onSearch(): void {
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }
    this.searchTimeout = setTimeout(() => {
      this.currentPage = 1;
      this.loadUsers();
    }, 400);
  }

  getRoleSeverity(role: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    switch (role) {
      case UserRole.SuperAdmin: return 'danger';
      case UserRole.SupportAdmin: return 'warn';
      case UserRole.QuizAuthor: return 'info';
      case UserRole.Examine: return 'success';
      default: return 'secondary';
    }
  }

  viewUser(user: UserListItem): void {
    this.selectedUser.set(user);
    this.displayDetailDialog = true;
  }

  editRole(user: UserListItem): void {
    this.selectedUser.set(user);
    this.selectedRole = user.roles.length > 0 ? user.roles[0] : '';
    this.displayRoleDialog = true;
  }

  updateRole(): void {
    const user = this.selectedUser();
    if (!user || !this.selectedRole) return;

    this.identityService.updateUserRole(user.userId, { roleName: this.selectedRole }).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'User role updated successfully.' });
        this.displayRoleDialog = false;
        this.loadUsers();
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update user role.' });
      },
    });
  }

  confirmDelete(user: UserListItem): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete ${user.firstName} ${user.lastName}?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.identityService.deleteUser(user.userId).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Deleted', detail: 'User deleted successfully.' });
            this.loadUsers();
          },
          error: () => {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete user.' });
          },
        });
      },
    });
  }

  private loadUsers(): void {
    this.loading.set(true);
    this.identityService.getUsers(this.currentPage, this.pageSize).subscribe({
      next: (response) => {
        let items = response.items;
        if (this.searchTerm) {
          const term = this.searchTerm.toLowerCase();
          items = items.filter(u =>
            u.firstName.toLowerCase().includes(term) ||
            u.lastName.toLowerCase().includes(term) ||
            (u.email?.toLowerCase().includes(term) ?? false)
          );
        }
        this.users.set(items);
        this.totalRecords.set(response.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }
}
