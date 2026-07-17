import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LoadingController, ToastController } from '@ionic/angular';
import { ApiService } from '@core/services/api.service';
import { Order } from '@core/models';

@Component({
  standalone: false,
  selector: 'app-order-detail',
  templateUrl: './order-detail.page.html',
  styleUrls: ['./order-detail.page.scss'],
})
export class OrderDetailPage implements OnInit {
  order: Order | null = null;
  isLoading = true;

  timelineSteps = [
    { key: 'pending', label: 'Order Placed', icon: 'receipt-outline' },
    { key: 'confirmed', label: 'Confirmed', icon: 'checkmark-circle-outline' },
    { key: 'processing', label: 'Processing', icon: 'construct-outline' },
    { key: 'shipped', label: 'Shipped', icon: 'car-outline' },
    { key: 'delivered', label: 'Delivered', icon: 'home-outline' },
  ];

  private statusOrder = ['pending', 'confirmed', 'processing', 'shipped', 'delivered'];

  constructor(
    private route: ActivatedRoute,
    private api: ApiService,
    private loadingCtrl: LoadingController,
    private toastCtrl: ToastController
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadOrder(Number(id));
    }
  }

  loadOrder(id: number): void {
    this.isLoading = true;
    this.api.get<Order>(`/api/v1/orders/${id}`).subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res.success) {
          this.order = res.data;
        }
      },
      error: async () => {
        this.isLoading = false;
        const toast = await this.toastCtrl.create({
          message: 'Failed to load order details',
          duration: 2000,
          color: 'danger',
        });
        toast.present();
      },
    });
  }

  getTimelineStatus(status: string): 'completed' | 'current' | 'upcoming' {
    if (!this.order) return 'upcoming';
    const currentIdx = this.statusOrder.indexOf(this.order.status.toLowerCase());
    const stepIdx = this.statusOrder.indexOf(status.toLowerCase());
    if (stepIdx < currentIdx) return 'completed';
    if (stepIdx === currentIdx) return 'current';
    return 'upcoming';
  }

  getTrackingLatest(): string {
    if (!this.order?.tracking?.length) return '';
    return this.order.tracking[0]?.note ?? '';
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending': return '#f59e0b';
      case 'confirmed': return '#6366f1';
      case 'processing': return '#8b5cf6';
      case 'shipped': return '#0ea5e9';
      case 'delivered': return '#10b981';
      case 'cancelled': return '#ef4444';
      default: return '#64748b';
    }
  }

  getPaymentStatusColor(): string {
    if (!this.order) return '#64748b';
    switch (this.order.paymentStatus?.toLowerCase()) {
      case 'paid': return '#10b981';
      case 'pending': return '#f59e0b';
      case 'failed': return '#ef4444';
      default: return '#64748b';
    }
  }
}
