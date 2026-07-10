import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { Notification } from '@shared/models';

@Component({
  standalone: false,
  selector: 'app-notification-dropdown',
  templateUrl: './notification-dropdown.component.html',
  styleUrls: ['./notification-dropdown.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotificationDropdownComponent {
  @Input() notifications: Notification[] = [];
  @Output() close = new EventEmitter<void>();

  constructor(private router: Router) {}

  navigateToOrder(orderId?: number): void {
    if (orderId) {
      this.router.navigate(['/profile/orders', orderId]);
    }
    this.close.emit();
  }

  getNotificationIcon(type: string): string {
    switch (type) {
      case 'order_confirmed': return 'bi bi-check-circle-fill text-success';
      case 'order_shipped': return 'bi bi-truck text-info';
      case 'order_delivered': return 'bi bi-box-seam text-success';
      case 'payment_received': return 'bi bi-credit-card text-primary';
      case 'low_stock': return 'bi bi-exclamation-triangle text-warning';
      case 'new_review': return 'bi bi-star text-warning';
      default: return 'bi bi-bell text-secondary';
    }
  }

  formatTimestamp(ts: string): string {
    const date = new Date(ts);
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);

    if (minutes < 1) return 'Just now';
    if (minutes < 60) return `${minutes}m ago`;
    if (hours < 24) return `${hours}h ago`;
    if (days < 7) return `${days}d ago`;
    return date.toLocaleDateString();
  }
}
