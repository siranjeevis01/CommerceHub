import { Component, Input, ChangeDetectionStrategy } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-price-display',
  templateUrl: './price-display.component.html',
  styleUrls: ['./price-display.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PriceDisplayComponent {
  @Input() price = 0;
  @Input() comparePrice?: number;
  @Input() currency = 'USD';

  formatPrice(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: this.currency,
    }).format(value);
  }

  get discountPercent(): number | null {
    if (this.comparePrice && this.comparePrice > this.price) {
      return Math.round((1 - this.price / this.comparePrice) * 100);
    }
    return null;
  }
}
