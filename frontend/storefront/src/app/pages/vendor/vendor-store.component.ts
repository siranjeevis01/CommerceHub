import { Component, OnInit, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@shared/services/api.service';
import { CartService } from '@shared/services/cart.service';
import { VendorProfile, Product, PaginatedResponse } from '@shared/models';
import { Observable, Subject, of, combineLatest } from 'rxjs';
import { map, switchMap, takeUntil, catchError, shareReplay, tap, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { HttpParams } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-vendor-store',
  templateUrl: './vendor-store.component.html',
  styleUrls: ['./vendor-store.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VendorStoreComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  vendor$!: Observable<VendorProfile | null>;
  products$!: Observable<PaginatedResponse<Product>>;
  isLoading = true;
  vendorError = false;

  sortBy = 'newest';
  currentPage = 1;
  pageSize = 12;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private api: ApiService,
    private cartService: CartService,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    const slug$ = this.route.params.pipe(
      map(params => params['slug']),
      distinctUntilChanged(),
      shareReplay(1),
    );

    this.vendor$ = slug$.pipe(
      switchMap(slug => this.api.get<VendorProfile>(`/api/v1/vendors/${slug}`).pipe(
        map(r => r.data),
        catchError(() => {
          this.vendorError = true;
          this.isLoading = false;
          return of(null);
        }),
      )),
      tap(() => this.isLoading = false),
    );

    this.products$ = combineLatest([
      slug$,
      this.route.queryParams,
    ]).pipe(
      debounceTime(50),
      switchMap(([slug, qp]) => {
        this.currentPage = +qp['page'] || 1;
        this.sortBy = qp['sort'] || 'newest';

        let params = new HttpParams().set('sort', this.sortBy);
        return this.api.getPaginated<Product>(`/api/v1/vendors/${slug}/products`, this.currentPage, this.pageSize, params);
      }),
      tap(() => window.scrollTo({ top: 0, behavior: 'smooth' })),
    );
  }

  onSortChange(): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { sort: this.sortBy, page: 1 },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }

  onPageChange(page: number): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }

  addToCart(product: Product): void {
    this.cartService.addItem(product.id, product.name, product.imageUrl, product.price, 1).subscribe({
      next: () => this.toastr.success(`${product.name} added to cart`),
      error: () => this.toastr.error('Failed to add item'),
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
