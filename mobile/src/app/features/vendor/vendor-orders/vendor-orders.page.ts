import { Component, OnInit } from '@angular/core';
import { ApiService } from '@core/services/api.service';
import { Order } from '@core/models';

@Component({
  standalone: false,
  selector: 'app-vendor-orders',
  templateUrl: './vendor-orders.page.html',
  styleUrls: ['./vendor-orders.page.scss'],
})
export class VendorOrdersPage implements OnInit {
  orders: Order[] = [];
  isLoading = false;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading = true;
    this.api.get<Order[]>('/api/v1/vendor/orders').subscribe({
      next: (res) => {
        this.isLoading = false;
        this.orders = res?.data ?? [];
      },
      error: () => { this.isLoading = false; },
    });
  }

  updateStatus(order: Order, status: string): void {
    this.api.put(`/api/v1/vendor/orders/${order.id}/status`, { status }).subscribe({
      next: () => { order.status = status; },
    });
  }

  getStatusColor(status: string): string {
    switch (status?.toLowerCase()) {
      case 'pending': return 'warning';
      case 'processing': return 'tertiary';
      case 'shipped': return 'primary';
      case 'delivered': return 'success';
      case 'cancelled': return 'danger';
      default: return 'medium';
    }
  }
}
