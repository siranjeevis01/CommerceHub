import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

export type BadgeStatus = 'active' | 'inactive' | 'pending' | 'approved' | 'rejected' | 'completed' | 'processing' |
  'cancelled' | 'refunded' | 'shipped' | 'delivered' | 'paid' | 'unpaid' | 'draft' | 'published' | 'suspended' | 'verified' | 'unverified';

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
    const s = this.status?.toLowerCase() || '';
    if (['active', 'published', 'paid', 'completed', 'delivered', 'verified', 'approved'].includes(s)) return 'badge-success';
    if (['pending', 'processing', 'shipped'].includes(s)) return 'badge-warning';
    if (['inactive', 'draft'].includes(s)) return 'badge-secondary';
    if (['rejected', 'cancelled', 'refunded', 'suspended', 'unpaid', 'unverified'].includes(s)) return 'badge-danger';
    return 'badge-info';
  }

  get displayLabel(): string {
    return this.status ? this.status.charAt(0).toUpperCase() + this.status.slice(1) : 'Unknown';
  }
}
