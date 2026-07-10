import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { Observable, catchError, map, of, shareReplay, switchMap } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { Review, PaginatedResponse } from '@shared/models';
import { VendorService } from '../../services/vendor.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-review-list',
  templateUrl: './review-list.component.html',
  styleUrls: ['./review-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReviewListComponent implements OnInit {
  reviews$: Observable<PaginatedResponse<Review> | null>;
  loading = true;
  page = 1;
  pageSize = 20;
  ratingFilter = '';
  statusFilter = '';
  replyText: { [reviewId: number]: string } = {};
  replyingTo: number | null = null;

  constructor(
    private vendorService: VendorService,
    private toastr: ToastrService,
  ) {
    this.reviews$ = of(null);
  }

  ngOnInit(): void {
    this.loadReviews();
  }

  loadReviews(): void {
    this.loading = true;
    let params = new HttpParams();
    if (this.ratingFilter) params = params.set('rating', this.ratingFilter);
    if (this.statusFilter) params = params.set('status', this.statusFilter);

    this.reviews$ = this.vendorService.getReviews(this.page, this.pageSize, params).pipe(
      map(r => r),
      catchError(() => of(null)),
      shareReplay(1)
    );
    this.reviews$.subscribe(() => { this.loading = false; });
  }

  onRatingFilter(value: string): void {
    this.ratingFilter = value;
    this.page = 1;
    this.loadReviews();
  }

  onStatusFilter(value: string): void {
    this.statusFilter = value;
    this.page = 1;
    this.loadReviews();
  }

  onPageChange(newPage: number): void {
    this.page = newPage;
    this.loadReviews();
  }

  getPages(totalPages: number): number[] {
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }

  startReply(reviewId: number): void {
    this.replyingTo = reviewId;
    this.replyText[reviewId] = this.replyText[reviewId] || '';
  }

  cancelReply(): void {
    this.replyingTo = null;
  }

  submitReply(reviewId: number): void {
    const reply = this.replyText[reviewId]?.trim();
    if (!reply) { this.toastr.warning('Please enter a reply'); return; }

    this.vendorService.replyToReview(reviewId, reply).subscribe({
      next: () => {
        this.toastr.success('Reply submitted');
        this.replyingTo = null;
        this.replyText[reviewId] = '';
        this.loadReviews();
      },
      error: () => this.toastr.error('Failed to submit reply'),
    });
  }
}
