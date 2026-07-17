import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil, catchError, of } from 'rxjs';

import { ApiService } from '@core/services/api.service';
import { CartService } from '@core/services/cart.service';
import { Product } from '@core/models';

type SortOption = 'relevance' | 'price_asc' | 'price_desc' | 'rating' | 'newest';

@Component({
  standalone: false,
  selector: 'app-category-products',
  templateUrl: './category-products.page.html',
  styleUrls: ['./category-products.page.scss'],
})
export class CategoryProductsPage implements OnInit, OnDestroy {
  products: Product[] = [];
  categoryName = '';
  categoryId: number | null = null;

  selectedSort: SortOption = 'relevance';
  showSortOptions = false;

  isLoading = true;
  isRefreshing = false;
  hasMore = true;
  page = 1;
  pageSize = 20;
  totalProducts = 0;

  sortOptions: { value: SortOption; label: string; icon: string }[] = [
    { value: 'relevance', label: 'Relevance', icon: 'swap-vertical-outline' },
    { value: 'price_asc', label: 'Price: Low to High', icon: 'trending-up-outline' },
    { value: 'price_desc', label: 'Price: High to Low', icon: 'trending-down-outline' },
    { value: 'rating', label: 'Top Rated', icon: 'star-outline' },
    { value: 'newest', label: 'Newest First', icon: 'time-outline' },
  ];

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private api: ApiService,
    private cartService: CartService,
  ) {}

  ngOnInit(): void {
    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        if (params['id']) {
          this.categoryId = Number(params['id']);
        }
        if (params['name']) {
          this.categoryName = params['name'];
        }
        this.loadProducts();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProducts(): void {
    this.isLoading = true;
    this.page = 1;
    this.hasMore = true;

    const params: any = {
      page: this.page,
      pageSize: this.pageSize,
      sort: this.selectedSort,
    };
    if (this.categoryId) {
      params.categoryId = this.categoryId;
    }

    this.api.get<any>('/api/v1/products', params)
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of({ data: [], total: 0 })),
      )
      .subscribe({
        next: (res: any) => {
          this.products = res?.data ?? [];
          this.totalProducts = res?.total ?? 0;
          this.hasMore = this.products.length >= this.pageSize;
          this.isLoading = false;
        },
        error: () => {
          this.products = [];
          this.isLoading = false;
        },
      });
  }

  loadMore(event: any): void {
    if (!this.hasMore || this.isLoading) {
      event.target?.complete();
      return;
    }
    this.page++;

    const params: any = {
      page: this.page,
      pageSize: this.pageSize,
      sort: this.selectedSort,
    };
    if (this.categoryId) {
      params.categoryId = this.categoryId;
    }

    this.api.get<any>('/api/v1/products', params)
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of({ data: [] })),
      )
      .subscribe({
        next: (res: any) => {
          const newProducts = res?.data ?? [];
          this.products = [...this.products, ...newProducts];
          this.totalProducts = res?.total ?? this.totalProducts;
          this.hasMore = newProducts.length >= this.pageSize;
          event.target?.complete();
        },
        error: () => {
          event.target?.complete();
        },
      });
  }

  doRefresh(event: any): void {
    this.isRefreshing = true;
    this.loadProducts();
    event.target?.complete();
    this.isRefreshing = false;
  }

  selectSort(option: SortOption): void {
    this.selectedSort = option;
    this.showSortOptions = false;
    this.loadProducts();
  }

  toggleSortOptions(): void {
    this.showSortOptions = !this.showSortOptions;
  }

  closeSortPanel(): void {
    this.showSortOptions = false;
  }

  navigateToProduct(product: Product): void {
    this.router.navigate(['/product', product.id]);
  }

  addToCart(event: Event, product: Product): void {
    event.stopPropagation();
    this.cartService.addToCart(product.id, 1);
  }

  getDiscountPercent(product: Product): number {
    if (product.comparePrice && product.comparePrice > product.price) {
      return Math.round(((product.comparePrice - product.price) / product.comparePrice) * 100);
    }
    return 0;
  }

  getRatingArray(rating: number): number[] {
    return Array.from({ length: 5 }, (_, i) => (i < Math.floor(rating) ? 1 : 0));
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(price);
  }

  trackByProduct(index: number, item: Product): number {
    return item.id;
  }

  trackBySort(index: number, item: any): string {
    return item.value;
  }
}
