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
import { MultiSelect } from 'primeng/multiselect';
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
    MultiSelect,
    InputText,
    ConfirmDialog,
    IconField,
    InputIcon,
  ],
  providers: [ConfirmationService, MessageService],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss',
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
  selectedRoles: string[] = [];
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
    this.selectedRoles = [...user.roles];
    this.displayRoleDialog = true;
  }

  updateRole(): void {
    const user = this.selectedUser();
    if (!user || !this.selectedRoles.length) return;

    this.identityService.updateUserRole(user.userId, { roleNames: this.selectedRoles }).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'User roles updated successfully.' });
        this.displayRoleDialog = false;
        this.loadUsers();
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update user roles.' });
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
