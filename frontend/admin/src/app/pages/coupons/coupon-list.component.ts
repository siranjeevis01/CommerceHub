import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ApiService } from '@shared/services/api.service';
import { Coupon } from '@shared/models';
import { Subject, takeUntil } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-coupon-list',
  templateUrl: './coupon-list.component.html',
  styleUrls: ['./coupon-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CouponListComponent implements OnInit, OnDestroy {
  coupons: Coupon[] = [];
  loading = false;
  error: string | null = null;
  showForm = false;
  editingCoupon: Coupon | null = null;

  form = {
    code: '',
    discountType: 'percentage' as 'percentage' | 'fixed',
    discountValue: 0,
    minimumOrderAmount: null as number | null,
    maxUsageCount: null as number | null,
    usedCount: 0,
    expiresAt: '',
    isValid: true,
  };

  private destroy$ = new Subject<void>();

  constructor(
    private api: ApiService,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.loadCoupons();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCoupons(): void {
    this.loading = true;
    this.error = null;
    this.api.get<Coupon[]>('/api/v1/admin/coupons')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.success) this.coupons = res.data;
          this.loading = false;
        },
        error: () => {
          this.error = 'Failed to load coupons';
          this.loading = false;
        },
      });
  }

  openCreateForm(): void {
    this.editingCoupon = null;
    this.resetForm();
    this.showForm = true;
    this.generateCode();
  }

  openEditForm(coupon: Coupon): void {
    this.editingCoupon = coupon;
    this.form = {
      code: coupon.code,
      discountType: coupon.discountType as 'percentage' | 'fixed',
      discountValue: coupon.discountValue,
      minimumOrderAmount: coupon.minimumOrderAmount ?? null,
      maxUsageCount: null,
      usedCount: 0,
      expiresAt: '',
      isValid: coupon.isValid,
    };
    this.showForm = true;
  }

  private resetForm(): void {
    this.form = { code: '', discountType: 'percentage', discountValue: 0, minimumOrderAmount: null, maxUsageCount: null, usedCount: 0, expiresAt: '', isValid: true };
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingCoupon = null;
  }

  generateCode(): void {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
    let code = '';
    for (let i = 0; i < 8; i++) code += chars.charAt(Math.floor(Math.random() * chars.length));
    this.form.code = code;
  }

  saveCoupon(): void {
    if (!this.form.code || this.form.discountValue <= 0) {
      this.toastr.error('Code and value are required');
      return;
    }

    const payload = { ...this.form };
    const request = this.editingCoupon
      ? this.api.put<Coupon>(`/api/v1/admin/coupons/${this.editingCoupon.id}`, payload)
      : this.api.post<Coupon>('/api/v1/admin/coupons', payload);

    request.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastr.success(this.editingCoupon ? 'Coupon updated' : 'Coupon created');
        this.cancelForm();
        this.loadCoupons();
      },
      error: () => this.toastr.error('Failed to save coupon'),
    });
  }

  toggleCoupon(coupon: Coupon): void {
    this.api.put(`/api/v1/admin/coupons/${coupon.id}`, { isValid: !coupon.isValid })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { coupon.isValid = !coupon.isValid; this.toastr.success('Coupon updated'); },
        error: () => this.toastr.error('Failed to update coupon'),
      });
  }

  deleteCoupon(coupon: Coupon): void {
    if (!confirm(`Delete coupon "${coupon.code}"?`)) return;
    this.api.delete(`/api/v1/admin/coupons/${coupon.id}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { this.toastr.success('Coupon deleted'); this.loadCoupons(); },
        error: () => this.toastr.error('Failed to delete coupon'),
      });
  }

  getDiscountLabel(coupon: Coupon): string {
    return coupon.discountType === 'percentage' ? `${coupon.discountValue}%` : `$${coupon.discountValue}`;
  }

  retry(): void { this.loadCoupons(); }
}
