import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ApiService } from '@shared/services/api.service';
import { Order, VendorProfile } from '@shared/models';
import { Subject, takeUntil } from 'rxjs';
import { Chart, registerables } from 'chart.js';
import { DashboardStats, RevenuePoint, SystemHealth } from '../../admin.models';

Chart.register(...registerables);

@Component({
  standalone: false,
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent implements OnInit, OnDestroy {
  loading = true;
  error: string | null = null;

  stats: DashboardStats = {
    totalUsers: 0, totalVendors: 0, totalProducts: 0,
    totalOrders: 0, totalRevenue: 0, pendingVendors: 0,
    activeUsers: 0, lowStockProducts: 0,
  };

  recentOrders: Order[] = [];
  topProducts: { name: string; sales: number; revenue: number }[] = [];
  pendingVendors: VendorProfile[] = [];
  systemHealth: SystemHealth = { cpu: 0, memory: 0, disk: 0, uptime: '0d', status: 'healthy' };
  revenueData: RevenuePoint[] = [];

  private chart: Chart | null = null;
  private destroy$ = new Subject<void>();

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadDashboard(): void {
    this.loading = true;
    this.error = null;

    this.api.get<DashboardStats>('/api/v1/admin/dashboard/stats')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.stats = res.data;
            this.loadRevenueChart();
            this.loadOrders();
            this.loadTopProducts();
            this.loadPendingVendors();
            this.loadSystemHealth();
          }
        },
        error: () => {
          this.error = 'Failed to load dashboard data';
          this.loading = false;
        },
      });
  }

  private loadRevenueChart(): void {
    this.api.get<RevenuePoint[]>('/api/v1/admin/dashboard/revenue')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.revenueData = res.data;
            this.initChart(res.data);
          }
          this.loading = false;
        },
        error: () => { this.loading = false; },
      });
  }

  private loadOrders(): void {
    this.api.get<Order[]>('/api/v1/admin/orders?pageSize=5')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => { if (res.success) this.recentOrders = res.data; },
      });
  }

  private loadTopProducts(): void {
    this.api.get<{ name: string; sales: number; revenue: number }[]>('/api/v1/admin/dashboard/top-products')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => { if (res.success) this.topProducts = res.data; },
      });
  }

  private loadPendingVendors(): void {
    this.api.get<VendorProfile[]>('/api/v1/admin/vendors/pending')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => { if (res.success) this.pendingVendors = res.data; },
      });
  }

  private loadSystemHealth(): void {
    this.api.get<SystemHealth>('/api/v1/admin/system/health')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => { if (res.success) this.systemHealth = res.data; },
      });
  }

  private initChart(data: RevenuePoint[]): void {
    setTimeout(() => {
      const canvas = document.getElementById('revenueChart') as HTMLCanvasElement;
      if (!canvas) return;
      this.chart?.destroy();

      this.chart = new Chart(canvas, {
        type: 'line',
        data: {
          labels: data.map(d => d.month),
          datasets: [{
            label: 'Revenue',
            data: data.map(d => d.revenue),
            borderColor: '#4f6ef7',
            backgroundColor: 'rgba(79,110,247,0.1)',
            fill: true,
            tension: 0.4,
            pointRadius: 4,
            pointBackgroundColor: '#4f6ef7',
          }],
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: { legend: { display: false } },
          scales: {
            y: { beginAtZero: true, grid: { color: 'rgba(0,0,0,0.04)' } },
            x: { grid: { display: false } },
          },
        },
      });
    }, 100);
  }

  getOrderStatusClass(status: string): string {
    const map: Record<string, string> = {
      pending: 'badge-warning', confirmed: 'badge-info',
      shipped: 'badge-primary', delivered: 'badge-success',
      cancelled: 'badge-danger',
    };
    return map[status?.toLowerCase()] || 'badge-secondary';
  }

  getHealthColor(status: string): string {
    const map: Record<string, string> = { healthy: 'success', warning: 'warning', critical: 'danger' };
    return map[status] || 'secondary';
  }

  approveVendor(vendorId: number): void {
    this.api.post(`/api/v1/admin/vendors/${vendorId}/approve`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: () => this.loadPendingVendors() });
  }

  rejectVendor(vendorId: number): void {
    this.api.post(`/api/v1/admin/vendors/${vendorId}/reject`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: () => this.loadPendingVendors() });
  }

  retry(): void {
    this.loadDashboard();
  }

  Math = Math;
}
