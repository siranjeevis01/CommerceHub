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
  @Input() prefix = '';
  @Input() suffix = '';
  @Input() icon = '';
  @Input() trend?: number;
  @Input() trendLabel = '';
  @Input() color: 'primary' | 'success' | 'warning' | 'danger' | 'info' = 'primary';
  @Input() loading = false;

  get trendUp(): boolean {
    return (this.trend ?? 0) >= 0;
  }

  get formattedValue(): string {
    if (typeof this.value === 'number' && this.prefix === '$') {
      return `${this.prefix}${this.value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
    }
    if (typeof this.value === 'number') {
      return `${this.prefix}${this.value.toLocaleString()}${this.suffix}`;
    }
    return `${this.prefix}${this.value}${this.suffix}`;
  }
}
