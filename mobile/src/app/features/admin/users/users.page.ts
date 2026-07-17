import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject, takeUntil, debounceTime, distinctUntilChanged, catchError, of } from 'rxjs';
import { LoadingController, ToastController, AlertController } from '@ionic/angular';

import { ApiService } from '@core/services/api.service';
import { User } from '@core/models';

type RoleFilter = 'all' | 'customer' | 'vendor' | 'admin';

@Component({
  standalone: false,
  selector: 'app-users',
  templateUrl: './users.page.html',
  styleUrls: ['./users.page.scss'],
})
export class UsersPage implements OnInit, OnDestroy {
  users: User[] = [];
  filteredUsers: User[] = [];
  searchQuery = '';
  selectedRole: RoleFilter = 'all';
  isLoading = true;
  isRefreshing = false;
  hasMore = true;
  page = 1;
  pageSize = 20;
  totalUsers = 0;

  roleFilters: { value: RoleFilter; label: string; icon: string }[] = [
    { value: 'all', label: 'All', icon: 'people-outline' },
    { value: 'customer', label: 'Customers', icon: 'person-outline' },
    { value: 'vendor', label: 'Vendors', icon: 'storefront-outline' },
    { value: 'admin', label: 'Admins', icon: 'shield-outline' },
  ];

  private destroy$ = new Subject<void>();
  private searchSubject$ = new Subject<string>();

  constructor(
    private api: ApiService,
    private toastCtrl: ToastController,
    private alertCtrl: AlertController,
  ) {}

  ngOnInit(): void {
    this.setupSearch();
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSearch(): void {
    this.searchSubject$
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(300),
        distinctUntilChanged(),
      )
      .subscribe(() => {
        this.applyFilters();
      });
  }

  loadUsers(): void {
    this.isLoading = true;
    this.page = 1;
    const params: any = {
      page: this.page,
      pageSize: this.pageSize,
    };
    if (this.selectedRole !== 'all') {
      params.role = this.selectedRole;
    }
    if (this.searchQuery.trim()) {
      params.q = this.searchQuery.trim();
    }

    this.api.get<any>('/api/v1/users', params)
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of({ data: [], total: 0 })),
      )
      .subscribe({
        next: (res: any) => {
          this.users = res?.data ?? [];
          this.totalUsers = res?.total ?? 0;
          this.filteredUsers = [...this.users];
          this.hasMore = this.users.length >= this.pageSize;
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
          this.users = [];
          this.filteredUsers = [];
        },
      });
  }

  loadMore(event: any): void {
    if (!this.hasMore || this.isLoading) {
      event.target?.complete();
      return;
    }
    this.page++;
    this.api.get<any>('/api/v1/users', { page: this.page, pageSize: this.pageSize })
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of({ data: [], total: 0 })),
      )
      .subscribe({
        next: (res: any) => {
          const newUsers = res?.data ?? [];
          this.users = [...this.users, ...newUsers];
          this.totalUsers = res?.total ?? this.totalUsers;
          this.hasMore = newUsers.length >= this.pageSize;
          this.applyFilters();
          event.target?.complete();
        },
        error: () => {
          event.target?.complete();
        },
      });
  }

  doRefresh(event: any): void {
    this.isRefreshing = true;
    this.loadUsers();
    event.target?.complete();
    this.isRefreshing = false;
  }

  onSearchInput(event: any): void {
    this.searchSubject$.next(event.detail.value ?? '');
  }

  selectRole(role: RoleFilter): void {
    this.selectedRole = role;
    this.loadUsers();
  }

  private applyFilters(): void {
    let result = [...this.users];
    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase();
      result = result.filter(u =>
        u.firstName.toLowerCase().includes(q) ||
        u.lastName.toLowerCase().includes(q) ||
        u.email.toLowerCase().includes(q)
      );
    }
    this.filteredUsers = result;
  }

  async toggleBlockUser(user: User): Promise<void> {
    const action = user.isActive ? 'block' : 'unblock';
    const alert = await this.alertCtrl.create({
      header: `${action === 'block' ? 'Block' : 'Unblock'} User`,
      message: `Are you sure you want to ${action} ${user.firstName} ${user.lastName}?`,
      cssClass: 'futuristic-alert',
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: action === 'block' ? 'Block' : 'Unblock',
          cssClass: action === 'block' ? 'alert-danger' : 'alert-success',
          handler: () => this.performBlockToggle(user),
        },
      ],
    });
    alert.present();
  }

  private performBlockToggle(user: User): void {
    const endpoint = user.isActive
      ? `/api/v1/users/${user.id}/block`
      : `/api/v1/users/${user.id}/unblock`;

    this.api.post(endpoint)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: async () => {
          user.isActive = !user.isActive;
          await this.showToast(
            `${user.firstName} ${user.lastName} has been ${user.isActive ? 'unblocked' : 'blocked'}`,
            user.isActive ? 'success' : 'warning'
          );
        },
        error: async () => {
          await this.showToast('Failed to update user status', 'danger');
        },
      });
  }

  getRoleLabel(user: User): string {
    if (user.roles?.includes('Admin')) return 'Admin';
    if (user.roles?.includes('Vendor')) return 'Vendor';
    return 'Customer';
  }

  getRoleColor(user: User): string {
    if (user.roles?.includes('Admin')) return '#8b5cf6';
    if (user.roles?.includes('Vendor')) return '#0ea5e9';
    return '#10b981';
  }

  getInitials(user: User): string {
    return `${(user.firstName?.[0] ?? '').toUpperCase()}${(user.lastName?.[0] ?? '').toUpperCase()}`;
  }

  getTimeAgo(dateStr: string): string {
    if (!dateStr) return '';
    const now = new Date();
    const date = new Date(dateStr);
    const diffMs = now.getTime() - date.getTime();
    const diffDays = Math.floor(diffMs / 86400000);
    if (diffDays < 1) return 'Today';
    if (diffDays === 1) return 'Yesterday';
    if (diffDays < 30) return `${diffDays}d ago`;
    if (diffDays < 365) return `${Math.floor(diffDays / 30)}mo ago`;
    return `${Math.floor(diffDays / 365)}y ago`;
  }

  trackByUser(index: number, item: User): number {
    return item.id;
  }

  trackByRole(index: number, item: any): string {
    return item.value;
  }

  private async showToast(message: string, color: string): Promise<void> {
    const toast = await this.toastCtrl.create({
      message,
      duration: 2000,
      color,
      position: 'top',
      cssClass: 'futuristic-toast',
    });
    toast.present();
  }
}
