import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { Observable, catchError, map, of, shareReplay } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { Order, PaginatedResponse } from '@shared/models';
import { VendorService } from '../../services/vendor.service';

@Component({
  standalone: false,
  selector: 'app-order-list',
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrderListComponent implements OnInit {
  orders$: Observable<PaginatedResponse<Order> | null>;
  loading = true;
  page = 1;
  pageSize = 20;
  statusFilter = '';

  constructor(private vendorService: VendorService) {
    this.orders$ = of(null);
  }

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    let params = new HttpParams();
    if (this.statusFilter) params = params.set('status', this.statusFilter);

    this.orders$ = this.vendorService.getOrders(this.page, this.pageSize, params).pipe(
      map(r => r),
      catchError(() => of(null)),
      shareReplay(1)
    );
    this.orders$.subscribe(() => { this.loading = false; });
  }

  onStatusFilter(value: string): void {
    this.statusFilter = value;
    this.page = 1;
    this.loadOrders();
  }

  onPageChange(newPage: number): void {
    this.page = newPage;
    this.loadOrders();
  }

  getPages(totalPages: number): number[] {
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }
}
