import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { LoadingController, ToastController } from '@ionic/angular';
import { ApiService } from '@core/services/api.service';
import { Order } from '@core/models';

@Component({
  standalone: false,
  selector: 'app-orders',
  templateUrl: './orders.page.html',
  styleUrls: ['./orders.page.scss'],
})
export class OrdersPage implements OnInit {
  orders: Order[] = [];
  filteredOrders: Order[] = [];
  selectedTab = 'all';
  isLoading = false;

  statusTabs = [
    { key: 'all', label: 'All' },
    { key: 'pending', label: 'Pending' },
    { key: 'processing', label: 'Processing' },
    { key: 'shipped', label: 'Shipped' },
    { key: 'delivered', label: 'Delivered' },
  ];

  constructor(
    private api: ApiService,
    private router: Router,
    private loadingCtrl: LoadingController,
    private toastCtrl: ToastController
  ) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(event?: any): void {
    this.isLoading = true;
    this.api.get<Order[]>('/api/v1/orders').subscribe({
      next: (res) => {
        this.isLoading = false;
        if (event) event.target.complete();
        if (res.success) {
          this.orders = res.data ?? [];
          this.filterOrders();
        }
      },
      error: () => {
        this.isLoading = false;
        if (event) event.target.complete();
      },
    });
  }

  selectTab(key: string): void {
    this.selectedTab = key;
    this.filterOrders();
  }

  filterOrders(): void {
    if (this.selectedTab === 'all') {
      this.filteredOrders = this.orders;
    } else {
      this.filteredOrders = this.orders.filter(
        o => o.status.toLowerCase() === this.selectedTab.toLowerCase()
      );
    }
  }

  viewOrder(order: Order): void {
    this.router.navigate(['/order', order.id]);
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending': return '#f59e0b';
      case 'processing': return '#6366f1';
      case 'shipped': return '#0ea5e9';
      case 'delivered': return '#10b981';
      case 'cancelled': return '#ef4444';
      default: return '#64748b';
    }
  }

  getStatusIcon(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending': return 'time-outline';
      case 'processing': return 'sync-outline';
      case 'shipped': return 'car-outline';
      case 'delivered': return 'checkmark-circle-outline';
      case 'cancelled': return 'close-circle-outline';
      default: return 'help-circle-outline';
    }
  }

  trackByOrderId(_index: number, order: Order): number {
    return order.id;
  }

  getItemNames(order: Order): string {
    return order.items?.slice(0, 2).map(i => i.productName).join(', ') ?? '';
  }
}
