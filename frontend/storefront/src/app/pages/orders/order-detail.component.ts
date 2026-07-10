import { Component, OnInit, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '@shared/services/api.service';
import { Order } from '@shared/models';
import { Observable, Subject, of } from 'rxjs';
import { map, switchMap, takeUntil, catchError, shareReplay } from 'rxjs/operators';

@Component({
  standalone: false,
  selector: 'app-order-detail',
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrderDetailComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  order$!: Observable<Order | null>;
  isLoading = true;
  error = false;

  constructor(
    private route: ActivatedRoute,
    private api: ApiService,
  ) {}

  ngOnInit(): void {
    this.order$ = this.route.params.pipe(
      switchMap(params => this.api.get<Order>(`/api/v1/orders/${params['id']}`).pipe(
        map(r => r.data),
        catchError(() => {
          this.error = true;
          this.isLoading = false;
          return of(null);
        }),
      )),
      shareReplay(1),
      takeUntil(this.destroy$),
    );

    this.order$.subscribe(() => this.isLoading = false);
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

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
