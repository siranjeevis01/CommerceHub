import { Component, Input, ChangeDetectionStrategy } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-product-rating',
  templateUrl: './product-rating.component.html',
  styleUrls: ['./product-rating.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductRatingComponent {
  @Input() rating = 0;
  @Input() count = 0;
  @Input() showCount = true;
  @Input() size: 'sm' | 'md' | 'lg' = 'sm';
  Math = Math;

  get stars(): number[] {
    return [1, 2, 3, 4, 5];
  }

  starClass(star: number): string {
    if (star <= Math.floor(this.rating)) return 'bi bi-star-fill';
    if (star === Math.ceil(this.rating) && this.rating % 1 >= 0.5) return 'bi bi-star-half';
    return 'bi bi-star';
  }
}
