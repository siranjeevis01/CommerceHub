import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { Observable, catchError, forkJoin, map, of, shareReplay } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { VendorService, CommissionEntry } from '../../services/vendor.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-commission-report',
  templateUrl: './commission-report.component.html',
  styleUrls: ['./commission-report.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CommissionReportComponent implements OnInit {
  commissions$: Observable<{ data: CommissionEntry[]; total: number; totalPages: number; pageSize: number } | null>;
  summary$: Observable<{ total: number; thisMonth: number; pending: number } | null>;
  loading = true;
  page = 1;
  pageSize = 20;
  startDate = '';
  endDate = '';

  constructor(
    private vendorService: VendorService,
    private toastr: ToastrService,
  ) {
    this.commissions$ = of(null);
    this.summary$ = of(null);
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    let params = new HttpParams();
    if (this.startDate) params = params.set('startDate', this.startDate);
    if (this.endDate) params = params.set('endDate', this.endDate);

    forkJoin({
      commissions: this.vendorService.getCommissions(this.page, this.pageSize, params).pipe(
        map(r => ({ data: r.data, total: r.total, totalPages: r.totalPages, pageSize: r.pageSize })),
        catchError(() => of({ data: [], total: 0, totalPages: 0, pageSize: 20 })),
      ),
      summary: this.vendorService.getCommissionSummary().pipe(
        map(r => r.data),
        catchError(() => of({ total: 0, thisMonth: 0, pending: 0 })),
      ),
    }).subscribe({
      next: (res) => {
        this.commissions$ = of(res.commissions);
        this.summary$ = of(res.summary);
        this.loading = false;
      },
      error: () => { this.loading = false; },
    });
  }

  filter(): void {
    this.page = 1;
    this.loadData();
  }

  clearFilters(): void {
    this.startDate = '';
    this.endDate = '';
    this.page = 1;
    this.loadData();
  }

  onPageChange(newPage: number): void {
    this.page = newPage;
    this.loadData();
  }

  getPages(totalPages: number): number[] {
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }

  exportCSV(): void {
    this.toastr.info('Export feature coming soon');
  }
}
