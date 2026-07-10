import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ApiService } from '@shared/services/api.service';
import { User } from '@shared/models';
import { Subject, takeUntil } from 'rxjs';
import { TableColumn, TableAction, FilterOption } from '../../admin.models';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserListComponent implements OnInit, OnDestroy {
  users: User[] = [];
  total = 0;
  page = 1;
  pageSize = 10;
  loading = false;
  error: string | null = null;
  selectedUsers: User[] = [];

  private destroy$ = new Subject<void>();

  filters: FilterOption[] = [
    { key: 'search', label: 'Search', type: 'text', placeholder: 'Search name, email...' },
    { key: 'role', label: 'Role', type: 'select', placeholder: 'All Roles', options: [
      { label: 'Admin', value: 'Admin' }, { label: 'Vendor', value: 'Vendor' }, { label: 'Customer', value: 'Customer' },
    ]},
    { key: 'isActive', label: 'Status', type: 'select', placeholder: 'All Status', options: [
      { label: 'Active', value: true }, { label: 'Inactive', value: false },
    ]},
  ];

  columns: TableColumn[] = [
    { key: 'firstName', label: 'Name', sortable: true },
    { key: 'email', label: 'Email', sortable: true },
    { key: 'roles', label: 'Roles', type: 'badge' },
    { key: 'isActive', label: 'Status', type: 'badge' },
    { key: 'createdAt', label: 'Joined', type: 'date', sortable: true },
  ];

  actions: TableAction[] = [
    { label: 'Edit', icon: 'bi bi-pencil', class: 'btn-outline-primary', handler: (row) => this.router.navigate(['/users', row.id]) },
    { label: 'Suspend', icon: 'bi bi-pause-circle', class: 'btn-outline-warning', handler: (row) => this.toggleStatus(row),
      visible: (row) => row.isActive },
    { label: 'Activate', icon: 'bi bi-play-circle', class: 'btn-outline-success', handler: (row) => this.toggleStatus(row),
      visible: (row) => !row.isActive },
    { label: 'Delete', icon: 'bi bi-trash', class: 'btn-outline-danger', handler: (row) => this.deleteUser(row) },
  ];

  constructor(
    private api: ApiService,
    public router: Router,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadUsers(): void {
    this.loading = true;
    this.error = null;
    this.api.getPaginated<User>('/api/v1/admin/users', this.page, this.pageSize)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.users = res.data;
          this.total = res.total;
          this.loading = false;
        },
        error: () => {
          this.error = 'Failed to load users';
          this.loading = false;
        },
      });
  }

  onPageChange(page: number): void {
    this.page = page;
    this.loadUsers();
  }

  onSortChange(sort: { key: string; dir: 'asc' | 'desc' }): void {
    this.loadUsers();
  }

  onFilterChange(filters: Record<string, any>): void {
    this.page = 1;
    this.loadUsers();
  }

  onSelectionChange(selected: any[]): void {
    this.selectedUsers = selected;
  }

  bulkActivate(): void {
    if (this.selectedUsers.length === 0) return;
    this.api.post('/api/v1/admin/users/bulk/activate', { userIds: this.selectedUsers.map(u => u.id) })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastr.success(`${this.selectedUsers.length} users activated`);
          this.loadUsers();
        },
        error: () => this.toastr.error('Failed to activate users'),
      });
  }

  bulkDeactivate(): void {
    if (this.selectedUsers.length === 0) return;
    this.api.post('/api/v1/admin/users/bulk/deactivate', { userIds: this.selectedUsers.map(u => u.id) })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastr.success(`${this.selectedUsers.length} users deactivated`);
          this.loadUsers();
        },
        error: () => this.toastr.error('Failed to deactivate users'),
      });
  }

  toggleStatus(user: User): void {
    const action = user.isActive ? 'deactivate' : 'activate';
    this.api.post(`/api/v1/admin/users/${user.id}/${action}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastr.success(`User ${action}d`);
          this.loadUsers();
        },
        error: () => this.toastr.error(`Failed to ${action} user`),
      });
  }

  deleteUser(user: User): void {
    if (!confirm(`Delete user ${user.firstName} ${user.lastName}?`)) return;
    this.api.delete(`/api/v1/admin/users/${user.id}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastr.success('User deleted');
          this.loadUsers();
        },
        error: () => this.toastr.error('Failed to delete user'),
      });
  }

  retry(): void { this.loadUsers(); }
}
