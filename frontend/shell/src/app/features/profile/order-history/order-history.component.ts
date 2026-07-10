import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { ApiService } from '@shared/services/api.service';
import { Order, PaginatedResponse } from '@shared/models';
import { ColumnConfig, SortEvent, PageEvent } from '../../../shared/data-table/data-table.component';

@Component({
  standalone: false,
  selector: 'app-order-history',
  templateUrl: './order-history.component.html',
  styleUrls: ['./order-history.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OrderHistoryComponent implements OnInit {
  orders: Order[] = [];
  selectedOrder: Order | null = null;
  loading = false;
  total = 0;
  page = 1;
  pageSize = 10;

  columns: ColumnConfig[] = [
    { key: 'orderNumber', label: 'Order #', sortable: true, width: '140px' },
    { key: 'status', label: 'Status', sortable: true, width: '120px' },
    { key: 'totalAmount', label: 'Total', sortable: true, align: 'end', width: '100px' },
    { key: 'paymentStatus', label: 'Payment', sortable: true, width: '110px' },
    { key: 'createdAt', label: 'Date', sortable: true, width: '160px' }
  ];

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    const orderId = this.route.snapshot.paramMap.get('id');
    if (orderId) {
      this.loadOrderDetail(+orderId);
    } else {
      this.loadOrders();
    }
  }

  private loadOrders(): void {
    this.loading = true;
    this.api.getPaginated<Order>('/api/v1/orders', this.page, this.pageSize).subscribe({
      next: (res) => {
        this.orders = res.data;
        this.total = res.total || this.orders.length;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  private loadOrderDetail(id: number): void {
    this.loading = true;
    this.api.get<Order>(`/api/v1/orders/${id}`).subscribe({
      next: (res) => {
        this.selectedOrder = res.data;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  viewOrder(order: Order): void {
    this.selectedOrder = order;
  }

  backToList(): void {
    this.selectedOrder = null;
  }

  onSort(event: SortEvent): void {
    this.loadOrders();
  }

  onPage(event: PageEvent): void {
    this.page = event.page;
    this.pageSize = event.pageSize;
    this.loadOrders();
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      'Pending': 'bg-warning text-dark',
      'Confirmed': 'bg-info text-white',
      'Processing': 'bg-primary text-white',
      'Shipped': 'bg-info text-white',
      'Delivered': 'bg-success text-white',
      'Cancelled': 'bg-danger text-white',
      'Refunded': 'bg-secondary text-white'
    };
    return map[status] || 'bg-light text-dark';
  }

  getPaymentStatusClass(status: string): string {
    const map: Record<string, string> = {
      'Pending': 'bg-warning text-dark',
      'Paid': 'bg-success text-white',
      'Failed': 'bg-danger text-white',
      'Refunded': 'bg-secondary text-white'
    };
    return map[status] || 'bg-light text-dark';
  }
}
