import { Component, Input, ChangeDetectionStrategy } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-price-display',
  templateUrl: './price-display.component.html',
  styleUrls: ['./price-display.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PriceDisplayComponent {
  @Input() price = 0;
  @Input() comparePrice?: number;
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() currency = '$';
  @Input() showDiscount = true;

  get hasDiscount(): boolean {
    return !!this.comparePrice && this.comparePrice > this.price;
  }

  get discountPercent(): number {
    if (!this.hasDiscount) return 0;
    return Math.round(((this.comparePrice! - this.price) / this.comparePrice!) * 100);
  }
}
