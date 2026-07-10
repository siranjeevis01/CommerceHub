import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

type BadgeStatus = 'active' | 'inactive' | 'pending' | 'processing' | 'shipped' | 'delivered' | 'cancelled' | 'paid' | 'failed' | 'draft' | 'approved' | 'disapproved' | 'completed' | 'refunded';

@Component({
  standalone: false,
  selector: 'app-status-badge',
  templateUrl: './status-badge.component.html',
  styleUrls: ['./status-badge.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusBadgeComponent {
  @Input() status: BadgeStatus | string = 'active';
  @Input() size: 'sm' | 'md' = 'sm';

  get badgeClass(): string {
    const map: Record<string, string> = {
      active: 'badge-success',
      inactive: 'badge-secondary',
      draft: 'badge-secondary',
      pending: 'badge-warning',
      processing: 'badge-info',
      shipped: 'badge-info',
      delivered: 'badge-success',
      cancelled: 'badge-danger',
      paid: 'badge-success',
      failed: 'badge-danger',
      approved: 'badge-success',
      disapproved: 'badge-danger',
      completed: 'badge-success',
      refunded: 'badge-warning',
    };
    return map[this.status?.toLowerCase()] || 'badge-secondary';
  }

  get displayLabel(): string {
    return this.status?.replace(/_/g, ' ') || 'Unknown';
  }
}
