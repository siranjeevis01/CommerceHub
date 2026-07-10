import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-stat-card',
  templateUrl: './stat-card.component.html',
  styleUrls: ['./stat-card.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatCardComponent {
  @Input() label = '';
  @Input() value: number | string = 0;
  @Input() icon = '';
  @Input() change = 0;
  @Input() changeLabel = 'vs last period';
  @Input() color: 'primary' | 'success' | 'warning' | 'danger' | 'info' = 'primary';
  @Input() loading = false;
  @Input() prefix = '';
  @Input() suffix = '';

  get trend(): 'up' | 'down' | 'stable' {
    if (this.change > 0) return 'up';
    if (this.change < 0) return 'down';
    return 'stable';
  }

  get formattedChange(): string {
    const abs = Math.abs(this.change);
    return `${this.change >= 0 ? '+' : '-'}${abs.toFixed(1)}%`;
  }

  get displayValue(): string {
    if (typeof this.value === 'number') {
      if (this.value >= 1000000) return `${(this.value / 1000000).toFixed(1)}M`;
      if (this.value >= 1000) return `${(this.value / 1000).toFixed(1)}K`;
      return this.value.toLocaleString();
    }
    return this.value;
  }
}
