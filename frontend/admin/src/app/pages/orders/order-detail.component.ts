import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@shared/services/api.service';
import { Order, OrderTracking } from '@shared/models';
import { Subject, switchMap, takeUntil } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-order-detail',
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrderDetailComponent implements OnInit, OnDestroy {
  order: Order | null = null;
  loading = true;
  error: string | null = null;
  selectedStatus = '';
  statusOptions = ['pending', 'confirmed', 'processing', 'shipped', 'delivered', 'cancelled', 'refunded'];
  trackingNote = '';

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    public router: Router,
    private api: ApiService,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap(params => this.api.get<Order>(`/api/v1/admin/orders/${params.get('id')}`)),
      takeUntil(this.destroy$),
    ).subscribe({
      next: (res) => {
        if (res.success) {
          this.order = res.data;
          this.selectedStatus = res.data.status;
          this.loading = false;
        }
      },
      error: () => {
        this.error = 'Failed to load order';
        this.loading = false;
      },
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  updateStatus(): void {
    if (!this.order || this.selectedStatus === this.order.status) return;
    this.api.put(`/api/v1/admin/orders/${this.order.id}/status`, { status: this.selectedStatus, note: this.trackingNote })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          if (this.order) this.order.status = this.selectedStatus;
          this.toastr.success('Order status updated');
          this.trackingNote = '';
        },
        error: () => this.toastr.error('Failed to update status'),
      });
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      pending: 'badge-warning', confirmed: 'badge-info',
      processing: 'badge-info', shipped: 'badge-primary',
      delivered: 'badge-success', cancelled: 'badge-danger',
      refunded: 'badge-danger',
    };
    return map[status] || 'badge-secondary';
  }

  retry(): void {
    this.loading = true;
    this.error = null;
    this.ngOnInit();
  }
}
