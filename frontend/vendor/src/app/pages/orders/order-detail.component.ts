import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, catchError, map, of, shareReplay } from 'rxjs';
import { Order } from '@shared/models';
import { VendorService } from '../../services/vendor.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-order-detail',
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrderDetailComponent implements OnInit {
  order$: Observable<Order | null>;
  loading = true;
  selectedStatus = '';
  trackingNumber = '';
  showTrackingInput = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private vendorService: VendorService,
    private toastr: ToastrService,
  ) {
    const id = +this.route.snapshot.paramMap.get('id')!;
    this.order$ = this.vendorService.getOrder(id).pipe(
      map(r => r.data),
      catchError(() => {
        this.toastr.error('Order not found');
        this.router.navigate(['/orders']);
        return of(null);
      }),
      shareReplay(1)
    );
  }

  ngOnInit(): void {
    this.order$.subscribe(order => {
      this.loading = false;
      if (order) {
        this.selectedStatus = order.status;
        const tracking = order.tracking?.find(t => t.trackingNumber);
        if (tracking) {
          this.trackingNumber = tracking.trackingNumber || '';
        }
      }
    });
  }

  updateStatus(order: Order): void {
    if (!this.selectedStatus || this.selectedStatus === order.status) return;

    if (this.selectedStatus === 'shipped' && !this.trackingNumber) {
      this.showTrackingInput = true;
      this.toastr.warning('Please provide a tracking number');
      return;
    }

    this.vendorService.updateOrderStatus(order.id, this.selectedStatus, this.trackingNumber).subscribe({
      next: () => {
        this.toastr.success('Order status updated');
        location.reload();
      },
      error: (err) => this.toastr.error(err.error?.message || 'Failed to update status'),
    });
  }
}
