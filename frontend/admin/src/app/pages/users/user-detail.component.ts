import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@shared/services/api.service';
import { AuthService } from '@shared/services/auth.service';
import { User, Order } from '@shared/models';
import { Subject, switchMap, takeUntil } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-user-detail',
  templateUrl: './user-detail.component.html',
  styleUrls: ['./user-detail.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserDetailComponent implements OnInit, OnDestroy {
  user: User | null = null;
  orders: Order[] = [];
  loading = true;
  error: string | null = null;
  editingRoles = false;
  availableRoles = ['Customer', 'Vendor', 'Admin'];

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    public router: Router,
    private api: ApiService,
    private toastr: ToastrService,
    public auth: AuthService,
  ) {}

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap(params => this.api.get<User>(`/api/v1/admin/users/${params.get('id')}`)),
      takeUntil(this.destroy$),
    ).subscribe({
      next: (res) => {
        if (res.success) {
          this.user = res.data;
          this.loading = false;
          this.loadUserOrders();
        }
      },
      error: () => {
        this.error = 'Failed to load user';
        this.loading = false;
      },
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadUserOrders(): void {
    if (!this.user) return;
    this.api.get<Order[]>(`/api/v1/admin/users/${this.user.id}/orders`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => { if (res.success) this.orders = res.data; },
      });
  }

  toggleUserStatus(): void {
    if (!this.user) return;
    const action = this.user.isActive ? 'deactivate' : 'activate';
    this.api.post(`/api/v1/admin/users/${this.user.id}/${action}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          if (this.user) this.user.isActive = !this.user.isActive;
          this.toastr.success(`User ${action}d`);
        },
        error: () => this.toastr.error(`Failed to ${action} user`),
      });
  }

  updateRole(role: string, add: boolean): void {
    if (!this.user) return;
    const endpoint = add ? 'add-role' : 'remove-role';
    this.api.post(`/api/v1/admin/users/${this.user.id}/${endpoint}`, { role })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          if (add && !this.user!.roles.includes(role)) this.user!.roles.push(role);
          else if (!add) this.user!.roles = this.user!.roles.filter(r => r !== role);
          this.toastr.success(`Role ${add ? 'added' : 'removed'}`);
        },
        error: () => this.toastr.error('Failed to update role'),
      });
  }

  deleteUser(): void {
    if (!this.user || !confirm(`Delete user ${this.user.firstName} ${this.user.lastName}?`)) return;
    this.api.delete(`/api/v1/admin/users/${this.user.id}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastr.success('User deleted');
          this.router.navigate(['/users']);
        },
        error: () => this.toastr.error('Failed to delete user'),
      });
  }

  retry(): void {
    this.loading = true;
    this.error = null;
    this.ngOnInit();
  }
}
