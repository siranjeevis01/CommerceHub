import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { Observable, catchError, forkJoin, map, of, shareReplay } from 'rxjs';
import { VendorPayout } from '@shared/models';
import { VendorService, PayoutSummary } from '../../services/vendor.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-payout-history',
  templateUrl: './payout-history.component.html',
  styleUrls: ['./payout-history.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PayoutHistoryComponent implements OnInit {
  payouts$: Observable<{ data: VendorPayout[]; total: number; totalPages: number; pageSize: number } | null>;
  summary$: Observable<PayoutSummary | null>;
  loading = true;
  loadingSummary = true;
  page = 1;
  pageSize = 20;
  showConfirmDialog = false;

  constructor(
    private vendorService: VendorService,
    private toastr: ToastrService,
  ) {
    this.payouts$ = of(null);
    this.summary$ = of(null);
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.loadingSummary = true;

    forkJoin({
      payouts: this.vendorService.getPayouts(this.page, this.pageSize).pipe(
        map(r => ({ data: r.data, total: r.total, totalPages: r.totalPages, pageSize: r.pageSize })),
        catchError(() => of({ data: [], total: 0, totalPages: 0, pageSize: 20 })),
      ),
      summary: this.vendorService.getPayoutSummary().pipe(
        map(r => r.data),
        catchError(() => of({ currentBalance: 0, pendingAmount: 0, availableForPayout: 0 })),
      ),
    }).subscribe({
      next: (res) => {
        this.payouts$ = of(res.payouts);
        this.summary$ = of(res.summary);
        this.loading = false;
        this.loadingSummary = false;
      },
      error: () => {
        this.loading = false;
        this.loadingSummary = false;
      },
    });
  }

  requestPayout(): void {
    this.showConfirmDialog = true;
  }

  confirmPayout(): void {
    this.showConfirmDialog = false;
    this.vendorService.requestPayout().subscribe({
      next: () => {
        this.toastr.success('Payout requested successfully');
        this.loadData();
      },
      error: (err) => this.toastr.error(err.error?.message || 'Failed to request payout'),
    });
  }

  onPageChange(newPage: number): void {
    this.page = newPage;
    this.loadData();
  }

  getPages(totalPages: number): number[] {
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }
}
