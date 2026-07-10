import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { Observable, catchError, map, of, shareReplay } from 'rxjs';
import { VendorService, AnalyticsData } from '../../services/vendor.service';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  standalone: false,
  selector: 'app-vendor-analytics',
  templateUrl: './vendor-analytics.component.html',
  styleUrls: ['./vendor-analytics.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VendorAnalyticsComponent implements OnInit {
  analytics$: Observable<AnalyticsData | null>;
  loading = true;

  private revenueChart?: Chart;
  private orderChart?: Chart;

  constructor(private vendorService: VendorService) {
    this.analytics$ = this.vendorService.getAnalytics().pipe(
      map(r => r.data),
      catchError(() => of(null)),
      shareReplay(1)
    );
  }

  ngOnInit(): void {
    this.analytics$.subscribe(data => {
      this.loading = false;
      if (data) setTimeout(() => this.initCharts(data), 100);
    });
  }

  private initCharts(data: AnalyticsData): void {
    this.initRevenueChart(data);
    this.initOrderChart(data);
  }

  private initRevenueChart(data: AnalyticsData): void {
    const canvas = document.getElementById('revenueChart') as HTMLCanvasElement;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    if (this.revenueChart) this.revenueChart.destroy();

    this.revenueChart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: data.revenue.map(r => r.month),
        datasets: [{
          label: 'Monthly Revenue',
          data: data.revenue.map(r => r.revenue),
          borderColor: '#4f46e5',
          backgroundColor: 'rgba(79, 70, 229, 0.08)',
          fill: true,
          tension: 0.4,
          pointRadius: 4,
          pointBackgroundColor: '#4f46e5',
        }],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          y: { beginAtZero: true, ticks: { callback: (v) => '$' + v } },
          x: { grid: { display: false } },
        },
      },
    });
  }

  private initOrderChart(data: AnalyticsData): void {
    const canvas = document.getElementById('orderChart') as HTMLCanvasElement;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    if (this.orderChart) this.orderChart.destroy();

    this.orderChart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: data.orderVolume.map(o => o.week),
        datasets: [{
          label: 'Orders',
          data: data.orderVolume.map(o => o.orders),
          backgroundColor: 'rgba(79, 70, 229, 0.7)',
          borderColor: '#4f46e5',
          borderWidth: 1,
          borderRadius: 4,
        }],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          y: { beginAtZero: true, ticks: { precision: 0 } },
          x: { grid: { display: false } },
        },
      },
    });
  }
}
