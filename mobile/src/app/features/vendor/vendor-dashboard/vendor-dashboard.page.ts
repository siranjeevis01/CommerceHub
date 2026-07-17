import { Component, OnInit } from '@angular/core';
import { ApiService } from '@core/services/api.service';

@Component({
  standalone: false,
  selector: 'app-vendor-dashboard',
  templateUrl: './vendor-dashboard.page.html',
  styleUrls: ['./vendor-dashboard.page.scss'],
})
export class VendorDashboardPage implements OnInit {
  summary: any = { totalSales: 0, totalOrders: 0, totalProducts: 0, pendingOrders: 0 };
  recentOrders: any[] = [];
  isLoading = false;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.isLoading = true;
    this.api.get<any>('/api/v1/vendor/dashboard').subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res.success && res.data) {
          this.summary = res.data.summary ?? this.summary;
          this.recentOrders = res.data.recentOrders ?? [];
        }
      },
      error: () => { this.isLoading = false; },
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
