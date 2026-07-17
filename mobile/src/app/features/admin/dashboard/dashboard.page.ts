import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject, takeUntil, catchError, of } from 'rxjs';
import { LoadingController, ToastController } from '@ionic/angular';

import { ApiService } from '@core/services/api.service';
import { Order, User, VendorProfile, Product } from '@core/models';

interface DashboardStats {
  totalSales: number;
  totalOrders: number;
  totalUsers: number;
  totalVendors: number;
  totalProducts: number;
  pendingVendorApprovals: number;
  activeUsersToday: number;
  revenueGrowth: number;
}

@Component({
  standalone: false,
  selector: 'app-dashboard',
  templateUrl: './dashboard.page.html',
  styleUrls: ['./dashboard.page.scss'],
})
export class DashboardPage implements OnInit, OnDestroy {
  stats: DashboardStats = {
    totalSales: 0,
    totalOrders: 0,
    totalUsers: 0,
    totalVendors: 0,
    totalProducts: 0,
    pendingVendorApprovals: 0,
    activeUsersToday: 0,
    revenueGrowth: 0,
  };

  recentOrders: Order[] = [];
  pendingVendors: VendorProfile[] = [];
  recentActivity: any[] = [];

  isLoading = true;
  isRefreshing = false;

  private destroy$ = new Subject<void>();

  constructor(
    private api: ApiService,
    private loadingCtrl: LoadingController,
    private toastCtrl: ToastController,
  ) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  doRefresh(event: any): void {
    this.isRefreshing = true;
    this.loadDashboard();
    event.target?.complete();
    this.isRefreshing = false;
  }

  loadDashboard(): void {
    this.isLoading = true;
    this.api.get<any>('/api/v1/analytics/dashboard')
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of({ data: null })),
      )
      .subscribe({
        next: (res: any) => {
          if (res?.data) {
            this.stats = {
              totalSales: res.data.totalSales ?? 0,
              totalOrders: res.data.totalOrders ?? 0,
              totalUsers: res.data.totalCustomers ?? res.data.totalUsers ?? 0,
              totalVendors: res.data.totalVendors ?? 0,
              totalProducts: res.data.totalProducts ?? 0,
              pendingVendorApprovals: res.data.pendingVendorApprovals ?? 0,
              activeUsersToday: res.data.activeUsersToday ?? 0,
              revenueGrowth: res.data.revenueGrowth ?? 0,
            };
            this.recentOrders = res.data.recentOrders ?? [];
            this.pendingVendors = res.data.pendingVendors ?? [];
            this.buildActivityFeed(res.data);
          }
          this.isLoading = false;
        },
        error: async () => {
          this.isLoading = false;
          await this.showToast('Failed to load dashboard data', 'danger');
        },
      });
  }

  private buildActivityFeed(data: any): void {
    this.recentActivity = [];

    if (data.recentOrders?.length) {
      data.recentOrders.slice(0, 5).forEach((order: Order) => {
        this.recentActivity.push({
          icon: 'cart-outline',
          color: '#6366f1',
          title: `New order #${order.orderNumber}`,
          detail: `$${order.totalAmount.toFixed(2)} - ${order.status}`,
          time: order.createdAt,
        });
      });
    }

    if (data.pendingVendors?.length) {
      data.pendingVendors.forEach((vendor: VendorProfile) => {
        this.recentActivity.push({
          icon: 'storefront-outline',
          color: '#f59e0b',
          title: `Vendor pending: ${vendor.storeName}`,
          detail: 'Awaiting approval',
          time: '',
        });
      });
    }

    this.recentActivity.sort((a, b) => {
      if (!a.time) return 1;
      if (!b.time) return -1;
      return new Date(b.time).getTime() - new Date(a.time).getTime();
    });
  }

  async approveVendor(vendor: VendorProfile): Promise<void> {
    this.api.post(`/api/v1/vendors/${vendor.id}/approve`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: async () => {
          await this.showToast(`${vendor.storeName} approved`, 'success');
          this.pendingVendors = this.pendingVendors.filter(v => v.id !== vendor.id);
          this.stats.pendingVendorApprovals = Math.max(0, this.stats.pendingVendorApprovals - 1);
        },
        error: async () => {
          await this.showToast('Failed to approve vendor', 'danger');
        },
      });
  }

  async rejectVendor(vendor: VendorProfile): Promise<void> {
    this.api.post(`/api/v1/vendors/${vendor.id}/reject`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: async () => {
          await this.showToast(`${vendor.storeName} rejected`, 'warning');
          this.pendingVendors = this.pendingVendors.filter(v => v.id !== vendor.id);
          this.stats.pendingVendorApprovals = Math.max(0, this.stats.pendingVendorApprovals - 1);
        },
        error: async () => {
          await this.showToast('Failed to reject vendor', 'danger');
        },
      });
  }

  formatCurrency(value: number): string {
    if (value >= 1_000_000) return `$${(value / 1_000_000).toFixed(1)}M`;
    if (value >= 1_000) return `$${(value / 1_000).toFixed(1)}K`;
    return `$${value.toFixed(2)}`;
  }

  formatNumber(value: number): string {
    if (value >= 1_000_000) return `${(value / 1_000_000).toFixed(1)}M`;
    if (value >= 1_000) return `${(value / 1_000).toFixed(1)}K`;
    return value.toLocaleString();
  }

  getStatusColor(status: string): string {
    switch (status?.toLowerCase()) {
      case 'pending': return '#f59e0b';
      case 'confirmed': return '#6366f1';
      case 'processing': return '#8b5cf6';
      case 'shipped': return '#0ea5e9';
      case 'delivered': return '#10b981';
      case 'cancelled': return '#ef4444';
      default: return '#64748b';
    }
  }

  getTimeAgo(dateStr: string): string {
    if (!dateStr) return '';
    const now = new Date();
    const date = new Date(dateStr);
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    const diffHrs = Math.floor(diffMins / 60);
    if (diffHrs < 24) return `${diffHrs}h ago`;
    const diffDays = Math.floor(diffHrs / 24);
    return `${diffDays}d ago`;
  }

  trackByActivity(index: number, item: any): number {
    return index;
  }

  trackByOrder(index: number, item: Order): number {
    return item.id;
  }

  trackByVendor(index: number, item: VendorProfile): number {
    return item.id;
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
