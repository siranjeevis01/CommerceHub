import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { IonContent } from '@ionic/angular';
import { Subject, takeUntil, debounceTime, distinctUntilChanged, switchMap, catchError, of } from 'rxjs';

import { ApiService } from '@core/services/api.service';
import { CartService } from '@core/services/cart.service';
import { Product } from '@core/models';

type SortOption = 'relevance' | 'price_asc' | 'price_desc' | 'rating' | 'newest';

@Component({
  standalone: false,
  selector: 'app-search',
  templateUrl: './search.page.html',
  styleUrls: ['./search.page.scss'],
})
export class SearchPage implements OnInit, OnDestroy {
  @ViewChild(IonContent, { static: false }) content!: IonContent;

  searchQuery = '';
  results: Product[] = [];
  categories: any[] = [];

  selectedCategory: string | null = null;
  selectedCategoryName = '';
  selectedSort: SortOption = 'relevance';
  showSortOptions = false;

  isLoading = false;
  isRefreshing = false;
  hasMore = true;
  page = 1;
  pageSize = 20;
  totalResults = 0;

  sortOptions: { value: SortOption; label: string; icon: string }[] = [
    { value: 'relevance', label: 'Relevance', icon: 'swap-vertical-outline' },
    { value: 'price_asc', label: 'Price: Low to High', icon: 'trending-up-outline' },
    { value: 'price_desc', label: 'Price: High to Low', icon: 'trending-down-outline' },
    { value: 'rating', label: 'Top Rated', icon: 'star-outline' },
    { value: 'newest', label: 'Newest First', icon: 'time-outline' },
  ];

  recentSearches: string[] = [];

  private destroy$ = new Subject<void>();
  private searchSubject$ = new Subject<string>();

  constructor(
    private apiService: ApiService,
    private cartService: CartService,
    private route: ActivatedRoute,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.loadRecentSearches();
    this.loadCategories();
    this.setupSearch();
    this.handleQueryParams();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSearch(): void {
    this.searchSubject$
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(400),
        distinctUntilChanged(),
      )
      .subscribe((query) => {
        this.searchQuery = query;
        this.page = 1;
        this.results = [];
        this.hasMore = true;
        if (query.trim().length > 0 || this.selectedCategory) {
          this.performSearch();
        } else {
          this.results = [];
          this.totalResults = 0;
        }
      });
  }

  private handleQueryParams(): void {
    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe((params) => {
        if (params['q']) {
          this.searchQuery = params['q'];
          this.performSearch();
        }
        if (params['category']) {
          this.selectedCategory = params['category'];
          this.selectedCategoryName = params['name'] || '';
          this.performSearch();
        }
        if (params['flash_deal']) {
          this.performFlashDealSearch();
        }
      });
  }

  onSearchInput(event: any): void {
    const query = event.detail.value || '';
    this.searchSubject$.next(query);
  }

  onSearchClear(): void {
    this.searchQuery = '';
    this.results = [];
    this.totalResults = 0;
    this.page = 1;
    this.hasMore = true;
    this.selectedCategory = null;
    this.selectedCategoryName = '';
  }

  performSearch(): void {
    if (this.isLoading) return;

    this.isLoading = true;
    const params: any = {
      q: this.searchQuery || undefined,
      page: this.page,
      limit: this.pageSize,
      sort: this.selectedSort,
    };

    if (this.selectedCategory) {
      params.category = this.selectedCategory;
    }

    this.apiService.get('/api/v1/products/search', params)
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of({ data: [], total: 0 })),
      )
      .subscribe({
        next: (res: any) => {
          const items = res?.data || [];
          if (this.page === 1) {
            this.results = items;
          } else {
            this.results = [...this.results, ...items];
          }
          this.totalResults = res?.total || this.results.length;
          this.hasMore = items.length >= this.pageSize;
          this.isLoading = false;

          if (this.searchQuery.trim()) {
            this.saveRecentSearch(this.searchQuery.trim());
          }
        },
        error: () => {
          this.isLoading = false;
          this.hasMore = false;
        },
      });
  }

  private performFlashDealSearch(): void {
    this.isLoading = true;
    this.apiService.get('/api/v1/products', { flash_deal: true, page: 1, limit: 20 })
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of({ data: [], total: 0 })),
      )
      .subscribe({
        next: (res: any) => {
          this.results = res?.data || [];
          this.totalResults = res?.total || this.results.length;
          this.hasMore = this.results.length >= this.pageSize;
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
        },
      });
  }

  doRefresh(event: any): void {
    this.isRefreshing = true;
    this.page = 1;
    this.hasMore = true;
    this.performSearch();
    event.target.complete();
    this.isRefreshing = false;
  }

  loadMore(event: any): void {
    if (!this.hasMore || this.isLoading) {
      event.target.complete();
      return;
    }
    this.page++;
    this.performSearch();
    event.target.complete();
  }

  selectCategory(category: any): void {
    if (this.selectedCategory === category.id) {
      this.selectedCategory = null;
      this.selectedCategoryName = '';
    } else {
      this.selectedCategory = category.id;
      this.selectedCategoryName = category.name;
    }
    this.page = 1;
    this.results = [];
    this.hasMore = true;
    this.performSearch();
  }

  clearCategory(): void {
    this.selectedCategory = null;
    this.selectedCategoryName = '';
    this.page = 1;
    this.results = [];
    this.hasMore = true;
    if (this.searchQuery.trim()) {
      this.performSearch();
    } else {
      this.results = [];
      this.totalResults = 0;
    }
  }

  selectSort(option: SortOption): void {
    this.selectedSort = option;
    this.showSortOptions = false;
    this.page = 1;
    this.results = [];
    this.hasMore = true;
    this.performSearch();
  }

  toggleSortOptions(): void {
    this.showSortOptions = !this.showSortOptions;
  }

  navigateToProduct(product: Product): void {
    this.router.navigate(['/product', product.id]);
  }

  addToCart(event: Event, product: Product): void {
    event.stopPropagation();
    this.cartService.addToCart(product.id, 1);
  }

  searchRecent(term: string): void {
    this.searchQuery = term;
    this.searchSubject$.next(term);
  }

  removeRecent(term: string): void {
    this.recentSearches = this.recentSearches.filter((s) => s !== term);
    localStorage.setItem('recentSearches', JSON.stringify(this.recentSearches));
  }

  private loadRecentSearches(): void {
    try {
      const stored = localStorage.getItem('recentSearches');
      this.recentSearches = stored ? JSON.parse(stored) : [];
    } catch {
      this.recentSearches = [];
    }
  }

  private saveRecentSearch(term: string): void {
    this.recentSearches = [term, ...this.recentSearches.filter((s) => s !== term)].slice(0, 8);
    localStorage.setItem('recentSearches', JSON.stringify(this.recentSearches));
  }

  private loadCategories(): void {
    this.apiService.get('/api/v1/categories', { limit: 20, active: true })
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of({ data: [] })),
      )
      .subscribe({
        next: (res: any) => {
          this.categories = res?.data || [];
        },
        error: () => {},
      });
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
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(price);
  }

  trackByProduct(index: number, item: Product): number {
    return item.id || index;
  }

  trackByCategory(index: number, item: any): number {
    return item.id || index;
  }

  trackBySort(index: number, item: any): string {
    return item.value;
  }
}
