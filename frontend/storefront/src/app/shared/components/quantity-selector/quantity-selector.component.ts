import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-quantity-selector',
  templateUrl: './quantity-selector.component.html',
  styleUrls: ['./quantity-selector.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuantitySelectorComponent {
  @Input() quantity = 1;
  @Input() min = 1;
  @Input() max = 99;
  @Input() disabled = false;
  @Output() quantityChange = new EventEmitter<number>();

  decrease(): void {
    if (this.quantity > this.min && !this.disabled) {
      this.quantityChange.emit(this.quantity - 1);
    }
  }

  increase(): void {
    if (this.quantity < this.max && !this.disabled) {
      this.quantityChange.emit(this.quantity + 1);
    }
  }

  onInputChange(value: number): void {
    const clamped = Math.max(this.min, Math.min(this.max, value || this.min));
    this.quantityChange.emit(clamped);
  }
}
