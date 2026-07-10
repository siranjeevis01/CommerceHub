import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, catchError, map, of, shareReplay } from 'rxjs';
import { Product, Review } from '@shared/models';
import { VendorService } from '../../services/vendor.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-product-detail',
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductDetailComponent implements OnInit {
  product$: Observable<Product | null>;
  loading = true;
  activeTab: 'details' | 'variants' | 'reviews' = 'details';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private vendorService: VendorService,
    private toastr: ToastrService,
  ) {
    const id = +this.route.snapshot.paramMap.get('id')!;
    this.product$ = this.vendorService.getProduct(id).pipe(
      map(r => r.data),
      catchError(() => {
        this.toastr.error('Product not found');
        this.router.navigate(['/products']);
        return of(null);
      }),
      shareReplay(1)
    );
  }

  ngOnInit(): void {
    this.product$.subscribe(() => { this.loading = false; });
  }

  toggleActive(product: Product): void {
    this.vendorService.updateProduct(product.id, { isActive: !product.isActive } as any).subscribe({
      next: () => {
        this.toastr.success(product.isActive ? 'Product deactivated' : 'Product activated');
        location.reload();
      },
      error: () => this.toastr.error('Failed to update status'),
    });
  }
}
