import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { Observable, catchError, forkJoin, map, of, shareReplay } from 'rxjs';
import { VendorService, DashboardStats, SalesDataPoint } from '../../services/vendor.service';
import { Order, Product, Review } from '@shared/models';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  standalone: false,
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent implements OnInit {
  stats$: Observable<DashboardStats | null>;
  salesData$: Observable<SalesDataPoint[]>;
  recentOrders$: Observable<Order[]>;
  lowStockProducts$: Observable<Product[]>;
  topProducts$: Observable<Product[]>;
  recentReviews$: Observable<Review[]>;
  loading = true;

  private chart?: Chart;

  constructor(private vendorService: VendorService) {
    this.stats$ = this.vendorService.getDashboardStats().pipe(
      map(r => r.data),
      catchError(() => of(null)),
      shareReplay(1)
    );
    this.salesData$ = this.vendorService.getSalesData(30).pipe(
      map(r => r.data),
      catchError(() => of([])),
      shareReplay(1)
    );
    this.recentOrders$ = this.vendorService.getRecentOrders(5).pipe(
      map(r => r.data),
      catchError(() => of([])),
      shareReplay(1)
    );
    this.lowStockProducts$ = this.vendorService.getLowStockProducts(10).pipe(
      map(r => r.data),
      catchError(() => of([])),
      shareReplay(1)
    );
    this.topProducts$ = this.vendorService.getTopProducts(5).pipe(
      map(r => r.data),
      catchError(() => of([])),
      shareReplay(1)
    );
    this.recentReviews$ = this.vendorService.getRecentReviews(5).pipe(
      map(r => r.data),
      catchError(() => of([])),
      shareReplay(1)
    );
  }

  ngOnInit(): void {
    forkJoin([
      this.stats$,
      this.salesData$,
      this.recentOrders$,
      this.lowStockProducts$,
      this.topProducts$,
      this.recentReviews$,
    ]).subscribe({
      next: ([stats]) => {
        this.loading = false;
        if (stats) setTimeout(() => this.initChart(), 100);
      },
      error: () => { this.loading = false; },
    });
  }

  private initChart(): void {
    this.salesData$.subscribe(data => {
      const canvas = document.getElementById('salesChart') as HTMLCanvasElement;
      if (!canvas) return;
      const ctx = canvas.getContext('2d');
      if (!ctx) return;
      if (this.chart) this.chart.destroy();

      this.chart = new Chart(ctx, {
        type: 'line',
        data: {
          labels: data.map(d => {
            const date = new Date(d.date);
            return `${date.getMonth() + 1}/${date.getDate()}`;
          }),
          datasets: [
            {
              label: 'Revenue',
              data: data.map(d => d.revenue),
              borderColor: '#4f46e5',
              backgroundColor: 'rgba(79, 70, 229, 0.08)',
              fill: true,
              tension: 0.4,
              pointRadius: 3,
              pointBackgroundColor: '#4f46e5',
            },
            {
              label: 'Orders',
              data: data.map(d => d.orders),
              borderColor: '#10b981',
              backgroundColor: 'rgba(16, 185, 129, 0.08)',
              fill: true,
              tension: 0.4,
              pointRadius: 3,
              pointBackgroundColor: '#10b981',
              yAxisID: 'y1',
            },
          ],
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          interaction: { intersect: false, mode: 'index' },
          plugins: { legend: { display: true, position: 'top' } },
          scales: {
            y: { beginAtZero: true, ticks: { callback: (v) => '$' + v } },
            y1: { beginAtZero: true, position: 'right', grid: { display: false }, ticks: { precision: 0 } },
            x: { grid: { display: false } },
          },
        },
      });
    });
  }
}
