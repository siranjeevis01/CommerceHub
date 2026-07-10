import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@shared/services/api.service';
import { Product, VendorProfile, VendorPayout } from '@shared/models';
import { Subject, switchMap, takeUntil } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-vendor-detail',
  templateUrl: './vendor-detail.component.html',
  styleUrls: ['./vendor-detail.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VendorDetailComponent implements OnInit, OnDestroy {
  vendor: VendorProfile | null = null;
  products: Product[] = [];
  payouts: VendorPayout[] = [];
  loading = true;
  error: string | null = null;
  activeTab: 'products' | 'orders' | 'payouts' = 'products';

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    public router: Router,
    private api: ApiService,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap(params => this.api.get<VendorProfile>(`/api/v1/admin/vendors/${params.get('id')}`)),
      takeUntil(this.destroy$),
    ).subscribe({
      next: (res) => {
        if (res.success) {
          this.vendor = res.data;
          this.loading = false;
          this.loadProducts();
          this.loadPayouts();
        }
      },
      error: () => {
        this.error = 'Failed to load vendor';
        this.loading = false;
      },
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadProducts(): void {
    if (!this.vendor) return;
    this.api.get<Product[]>(`/api/v1/admin/vendors/${this.vendor.id}/products`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => { if (res.success) this.products = res.data; },
      });
  }

  private loadPayouts(): void {
    if (!this.vendor) return;
    this.api.get<VendorPayout[]>(`/api/v1/admin/vendors/${this.vendor.id}/payouts`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => { if (res.success) this.payouts = res.data; },
      });
  }

  approveVendor(): void {
    if (!this.vendor) return;
    this.api.post(`/api/v1/admin/vendors/${this.vendor.id}/approve`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { this.vendor!.status = 'approved'; this.toastr.success('Vendor approved'); },
        error: () => this.toastr.error('Failed to approve vendor'),
      });
  }

  rejectVendor(): void {
    if (!this.vendor) return;
    this.api.post(`/api/v1/admin/vendors/${this.vendor.id}/reject`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { this.vendor!.status = 'rejected'; this.toastr.success('Vendor rejected'); },
        error: () => this.toastr.error('Failed to reject vendor'),
      });
  }

  suspendVendor(): void {
    if (!this.vendor) return;
    this.api.post(`/api/v1/admin/vendors/${this.vendor.id}/suspend`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { this.vendor!.status = 'suspended'; this.toastr.success('Vendor suspended'); },
        error: () => this.toastr.error('Failed to suspend vendor'),
      });
  }

  retry(): void {
    this.loading = true;
    this.error = null;
    this.ngOnInit();
  }
}
