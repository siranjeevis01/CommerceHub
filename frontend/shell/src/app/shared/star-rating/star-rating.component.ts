import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-star-rating',
  templateUrl: './star-rating.component.html',
  styleUrls: ['./star-rating.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StarRatingComponent {
  @Input() rating = 0;
  @Input() maxStars = 5;
  @Input() readonly = false;
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Output() ratingChange = new EventEmitter<number>();

  get stars(): { type: 'full' | 'half' | 'empty'; value: number }[] {
    const result: { type: 'full' | 'half' | 'empty'; value: number }[] = [];
    const full = Math.floor(this.rating);
    const hasHalf = this.rating - full >= 0.25 && this.rating - full < 0.75;
    const roundUp = this.rating - full >= 0.75;

    for (let i = 1; i <= this.maxStars; i++) {
      if (i <= full || (i === full + 1 && roundUp)) {
        result.push({ type: 'full', value: i });
      } else if (i === full + 1 && hasHalf) {
        result.push({ type: 'half', value: i });
      } else {
        result.push({ type: 'empty', value: i });
      }
    }
    return result;
  }

  setRating(value: number): void {
    if (!this.readonly) {
      this.rating = value;
      this.ratingChange.emit(value);
    }
  }
}
