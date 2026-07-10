import { Component, OnInit, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@shared/services/api.service';
import { CartService } from '@shared/services/cart.service';
import { Product, Category, Brand, PaginatedResponse } from '@shared/models';
import { Observable, Subject, BehaviorSubject, combineLatest } from 'rxjs';
import { map, switchMap, tap, takeUntil, distinctUntilChanged, debounceTime } from 'rxjs/operators';
import { HttpParams } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-product-list',
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private refreshTrigger = new BehaviorSubject<void>(undefined);

  products$!: Observable<PaginatedResponse<Product>>;
  categories$!: Observable<Category[]>;
  brands$!: Observable<Brand[]>;

  currentPage = 1;
  pageSize = 12;
  sortBy = 'newest';
  viewMode: 'grid' | 'list' = 'grid';
  categorySlug = '';
  showFilters = false;

  selectedCategory = '';
  selectedBrand = '';
  minPrice: number | null = null;
  maxPrice: number | null = null;
  minRating = 0;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private api: ApiService,
    private cartService: CartService,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.categories$ = this.api.get<Category[]>('/api/v1/categories').pipe(map(r => r.data));
    this.brands$ = this.api.get<Brand[]>('/api/v1/brands').pipe(map(r => r.data));

    this.products$ = combineLatest([
      this.route.params,
      this.route.queryParams,
      this.refreshTrigger,
    ]).pipe(
      distinctUntilChanged(),
      debounceTime(50),
      switchMap(([params, queryParams]) => {
        this.categorySlug = params['slug'] || '';
        this.currentPage = +queryParams['page'] || 1;
        this.sortBy = queryParams['sort'] || 'newest';
        this.selectedCategory = queryParams['category'] || '';
        this.selectedBrand = queryParams['brand'] || '';
        this.minPrice = queryParams['minPrice'] ? +queryParams['minPrice'] : null;
        this.maxPrice = queryParams['maxPrice'] ? +queryParams['maxPrice'] : null;
        this.minRating = +queryParams['minRating'] || 0;

        let httpParams = new HttpParams();
        if (this.categorySlug) httpParams = httpParams.set('categorySlug', this.categorySlug);
        if (this.selectedCategory) httpParams = httpParams.set('category', this.selectedCategory);
        if (this.selectedBrand) httpParams = httpParams.set('brand', this.selectedBrand);
        if (this.minPrice !== null) httpParams = httpParams.set('minPrice', this.minPrice.toString());
        if (this.maxPrice !== null) httpParams = httpParams.set('maxPrice', this.maxPrice.toString());
        if (this.minRating > 0) httpParams = httpParams.set('minRating', this.minRating.toString());
        httpParams = httpParams.set('sort', this.sortBy);

        return this.api.getPaginated<Product>('/api/v1/products', this.currentPage, this.pageSize, httpParams);
      }),
      tap(() => window.scrollTo({ top: 0, behavior: 'smooth' })),
    );
  }

  onSortChange(): void {
    this.updateQueryParams({ sort: this.sortBy, page: 1 });
  }

  onPageChange(page: number): void {
    this.updateQueryParams({ page });
  }

  toggleView(mode: 'grid' | 'list'): void {
    this.viewMode = mode;
  }

  applyFilters(): void {
    this.updateQueryParams({
      category: this.selectedCategory,
      brand: this.selectedBrand,
      minPrice: this.minPrice,
      maxPrice: this.maxPrice,
      minRating: this.minRating > 0 ? this.minRating : null,
      page: 1,
    });
    this.showFilters = false;
  }

  clearFilters(): void {
    this.selectedCategory = '';
    this.selectedBrand = '';
    this.minPrice = null;
    this.maxPrice = null;
    this.minRating = 0;
    this.applyFilters();
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
