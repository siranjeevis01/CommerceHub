import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '@shared/services/api.service';
import { Order } from '@shared/models';
import { Subject, takeUntil } from 'rxjs';
import { TableColumn, FilterOption } from '../../admin.models';

@Component({
  standalone: false,
  selector: 'app-order-list',
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrderListComponent implements OnInit, OnDestroy {
  orders: Order[] = [];
  total = 0;
  page = 1;
  pageSize = 10;
  loading = false;
  error: string | null = null;

  private destroy$ = new Subject<void>();

  filters: FilterOption[] = [
    { key: 'search', label: 'Search', type: 'text', placeholder: 'Order # or customer...' },
    { key: 'status', label: 'Status', type: 'select', placeholder: 'All Status', options: [
      { label: 'Pending', value: 'pending' }, { label: 'Confirmed', value: 'confirmed' },
      { label: 'Shipped', value: 'shipped' }, { label: 'Delivered', value: 'delivered' },
      { label: 'Cancelled', value: 'cancelled' }, { label: 'Refunded', value: 'refunded' },
    ]},
    { key: 'paymentStatus', label: 'Payment', type: 'select', placeholder: 'All Payments', options: [
      { label: 'Paid', value: 'paid' }, { label: 'Unpaid', value: 'unpaid' },
      { label: 'Refunded', value: 'refunded' }, { label: 'Failed', value: 'failed' },
    ]},
    { key: 'dateFrom', label: 'From', type: 'date' },
    { key: 'dateTo', label: 'To', type: 'date' },
  ];

  columns: TableColumn[] = [
    { key: 'orderNumber', label: 'Order #', sortable: true },
    { key: 'userId', label: 'Customer', format: (v) => `User #${v}` },
    { key: 'status', label: 'Status', type: 'badge' },
    { key: 'paymentStatus', label: 'Payment', type: 'badge' },
    { key: 'totalAmount', label: 'Total', type: 'currency', sortable: true },
    { key: 'createdAt', label: 'Date', type: 'date', sortable: true },
  ];

  constructor(
    private api: ApiService,
    public router: Router,
  ) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadOrders(): void {
    this.loading = true;
    this.error = null;
    this.api.getPaginated<Order>('/api/v1/admin/orders', this.page, this.pageSize)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.orders = res.data;
          this.total = res.total;
          this.loading = false;
        },
        error: () => {
          this.error = 'Failed to load orders';
          this.loading = false;
        },
      });
  }

  onPageChange(page: number): void { this.page = page; this.loadOrders(); }
  onSortChange(sort: { key: string; dir: 'asc' | 'desc' }): void { this.loadOrders(); }
  onFilterChange(filters: Record<string, any>): void { this.page = 1; this.loadOrders(); }
  retry(): void { this.loadOrders(); }
}
