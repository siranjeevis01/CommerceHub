import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ApiService } from '@shared/services/api.service';
import { Order, PaginatedResponse } from '@shared/models';
import { BehaviorSubject, Observable } from 'rxjs';
import { switchMap, shareReplay, tap } from 'rxjs/operators';

@Component({
  standalone: false,
  selector: 'app-order-history',
  templateUrl: './order-history.component.html',
  styleUrls: ['./order-history.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrderHistoryComponent implements OnInit {
  private pageSubject = new BehaviorSubject<number>(1);
  orders$!: Observable<PaginatedResponse<Order>>;
  isLoading = true;
  pageSize = 10;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.orders$ = this.pageSubject.pipe(
      switchMap(page => this.api.getPaginated<Order>('/api/v1/orders', page, this.pageSize)),
      tap(() => this.isLoading = false),
      shareReplay(1),
    );
  }

  onPageChange(page: number): void {
    this.pageSubject.next(page);
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      pending: 'bg-warning text-dark',
      confirmed: 'bg-info text-dark',
      processing: 'bg-primary text-white',
      shipped: 'bg-info text-white',
      delivered: 'bg-success text-white',
      cancelled: 'bg-danger text-white',
      refunded: 'bg-secondary text-white',
    };
    return map[status.toLowerCase()] || 'bg-secondary text-white';
  }
}
