import { Component, OnInit, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@shared/services/api.service';
import { CartService } from '@shared/services/cart.service';
import { Product, PaginatedResponse } from '@shared/models';
import { Observable, Subject, BehaviorSubject, combineLatest } from 'rxjs';
import { map, switchMap, tap, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { HttpParams } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { FormControl } from '@angular/forms';

@Component({
  standalone: false,
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  results$!: Observable<PaginatedResponse<Product>>;
  searchControl = new FormControl('');
  sortBy = 'relevance';
  viewMode: 'grid' | 'list' = 'grid';
  currentPage = 1;
  pageSize = 12;
  hasSearched = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private api: ApiService,
    private cartService: CartService,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.route.queryParams.pipe(
      takeUntil(this.destroy$),
    ).subscribe(params => {
      const q = params['q'] || '';
      this.searchControl.setValue(q, { emitEvent: false });
      this.currentPage = +params['page'] || 1;
      this.sortBy = params['sort'] || 'relevance';
    });

    this.results$ = combineLatest([
      this.route.queryParams,
    ]).pipe(
      debounceTime(100),
      distinctUntilChanged(),
      switchMap(([params]) => {
        const q = params['q'] || '';
        this.hasSearched = !!q;
        this.currentPage = +params['page'] || 1;
        this.sortBy = params['sort'] || 'relevance';

        if (!q) {
          return new BehaviorSubject<PaginatedResponse<Product>>({
            data: [], total: 0, page: 1, pageSize: this.pageSize, totalPages: 0,
          }).asObservable();
        }

        let httpParams = new HttpParams()
          .set('q', q)
          .set('sort', this.sortBy);
        return this.api.getPaginated<Product>('/api/v1/products/search', this.currentPage, this.pageSize, httpParams);
      }),
      tap(() => window.scrollTo({ top: 0, behavior: 'smooth' })),
    );
  }

  onSearch(): void {
    const q = this.searchControl.value?.trim() || '';
    this.router.navigate(['/search'], { queryParams: { q, page: 1, sort: this.sortBy } });
  }

  onPageChange(page: number): void {
    this.updateQueryParams({ page });
  }

  onSortChange(): void {
    this.updateQueryParams({ sort: this.sortBy, page: 1 });
  }

  toggleView(mode: 'grid' | 'list'): void {
    this.viewMode = mode;
  }

  addToCart(product: Product): void {
    this.cartService.addItem(product.id, product.name, product.imageUrl, product.price, 1).subscribe({
      next: () => this.toastr.success(`${product.name} added to cart`),
      error: () => this.toastr.error('Failed to add item'),
    });
  }

  private updateQueryParams(params: any): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: params,
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
