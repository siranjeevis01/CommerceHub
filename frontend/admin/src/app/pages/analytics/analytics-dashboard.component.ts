import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ApiService } from '@shared/services/api.service';
import { Subject, takeUntil } from 'rxjs';
import { Chart, registerables } from 'chart.js';
import { AnalyticsMetric, ChartData } from '../../admin.models';

Chart.register(...registerables);

@Component({
  standalone: false,
  selector: 'app-analytics-dashboard',
  templateUrl: './analytics-dashboard.component.html',
  styleUrls: ['./analytics-dashboard.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AnalyticsDashboardComponent implements OnInit, OnDestroy {
  loading = true;
  error: string | null = null;
  dateRange: '7d' | '30d' | '90d' | '1y' = '30d';

  metrics: AnalyticsMetric[] = [];
  revenueChart: Chart | null = null;
  ordersChart: Chart | null = null;
  topProductsChart: Chart | null = null;
  usersChart: Chart | null = null;

  private destroy$ = new Subject<void>();

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadAnalytics();
  }

  ngOnDestroy(): void {
    this.revenueChart?.destroy();
    this.ordersChart?.destroy();
    this.topProductsChart?.destroy();
    this.usersChart?.destroy();
    this.destroy$.next();
    this.destroy$.complete();
  }

  changeRange(range: '7d' | '30d' | '90d' | '1y'): void {
    this.dateRange = range;
    this.loadAnalytics();
  }

  private loadAnalytics(): void {
    this.loading = true;
    this.error = null;

    this.api.get<any>(`/api/v1/admin/analytics?range=${this.dateRange}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.success) {
            const d = res.data;
            this.metrics = d.metrics || [];
            setTimeout(() => {
              this.initRevenueChart(d.revenueData);
              this.initOrdersChart(d.ordersData);
              this.initProductsChart(d.topProducts);
              this.initUsersChart(d.usersGrowth);
            }, 100);
          }
          this.loading = false;
        },
        error: () => {
          this.error = 'Failed to load analytics';
          this.loading = false;
        },
      });
  }

  private initRevenueChart(data: any): void {
    const canvas = document.getElementById('revenueChart') as HTMLCanvasElement;
    if (!canvas) return;
    this.revenueChart?.destroy();
    this.revenueChart = new Chart(canvas, {
      type: 'line',
      data: {
        labels: data?.labels || [],
        datasets: [{
          label: 'Revenue',
          data: data?.values || [],
          borderColor: '#4f6ef7',
          backgroundColor: 'rgba(79,110,247,0.1)',
          fill: true,
          tension: 0.4,
        }],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: { y: { beginAtZero: true, grid: { color: 'rgba(0,0,0,0.04)' } }, x: { grid: { display: false } } },
      },
    });
  }

  private initOrdersChart(data: any): void {
    const canvas = document.getElementById('ordersChart') as HTMLCanvasElement;
    if (!canvas) return;
    this.ordersChart?.destroy();
    this.ordersChart = new Chart(canvas, {
      type: 'bar',
      data: {
        labels: data?.labels || [],
        datasets: [{
          label: 'Orders',
          data: data?.values || [],
          backgroundColor: 'rgba(79,110,247,0.6)',
          borderRadius: 4,
        }],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          y: { beginAtZero: true, grid: { color: 'rgba(0,0,0,0.04)' }, ticks: { stepSize: 1 } },
          x: { grid: { display: false } },
        },
      },
    });
  }

  private initProductsChart(data: any): void {
    const canvas = document.getElementById('productsChart') as HTMLCanvasElement;
    if (!canvas) return;
    this.topProductsChart?.destroy();
    const colors = ['#4f6ef7', '#22c55e', '#f59e0b', '#ef4444', '#3b82f6', '#8b5cf6', '#ec4899', '#14b8a6'];
    this.topProductsChart = new Chart(canvas, {
      type: 'doughnut',
      data: {
        labels: data?.labels || [],
        datasets: [{
          data: data?.values || [],
          backgroundColor: colors.slice(0, data?.labels?.length || 0),
        }],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'right', labels: { font: { size: 11 }, boxWidth: 12, padding: 8 } },
        },
      },
    });
  }

  private initUsersChart(data: any): void {
    const canvas = document.getElementById('usersChart') as HTMLCanvasElement;
    if (!canvas) return;
    this.usersChart?.destroy();
    this.usersChart = new Chart(canvas, {
      type: 'line',
      data: {
        labels: data?.labels || [],
        datasets: [{
          label: 'Users',
          data: data?.values || [],
          borderColor: '#22c55e',
          backgroundColor: 'rgba(34,197,94,0.1)',
          fill: true,
          tension: 0.4,
        }],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: { y: { beginAtZero: true, grid: { color: 'rgba(0,0,0,0.04)' } }, x: { grid: { display: false } } },
      },
    });
  }

  retry(): void { this.loadAnalytics(); }
}
