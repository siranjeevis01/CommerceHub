import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import { IonContent } from '@ionic/angular';
import { Subject, takeUntil, interval, switchMap, catchError, of, finalize } from 'rxjs';

import { ApiService } from '@core/services/api.service';
import { CartService } from '@core/services/cart.service';
import { AuthService } from '@core/services/auth.service';
import { ThemeService } from '@core/services/theme.service';
import { Product, Category } from '@core/models';

@Component({
  standalone: false,
  selector: 'app-home',
  templateUrl: './home.page.html',
  styleUrls: ['./home.page.scss'],
})
export class HomePage implements OnInit, OnDestroy {
  @ViewChild(IonContent, { static: false }) content!: IonContent;
  @ViewChild('bannerSlider', { static: false }) bannerSlider!: ElementRef;

  user: any = { firstName: 'Guest' };
  banners: any[] = [];
  categories: Category[] = [];
  flashDeals: Product[] = [];
  featuredProducts: Product[] = [];
  recommendedProducts: Product[] = [];

  isLoading = true;
  isRefreshing = false;
  flashDealTimeLeft = { hours: 5, minutes: 32, seconds: 17 };

  private destroy$ = new Subject<void>();
  bannerIndex = 0;

  constructor(
    private apiService: ApiService,
    private cartService: CartService,
    private authService: AuthService,
    private themeService: ThemeService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.loadUser();
    this.loadHomeData();
    this.startBannerAutoplay();
    this.startCountdown();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  doRefresh(event: any): void {
    this.isRefreshing = true;
    this.loadHomeData().add(() => {
      event.target.complete();
      this.isRefreshing = false;
    });
  }

  loadUser(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (user) => {
          if (user) {
            this.user = user;
          }
        },
        error: () => {},
      });
  }

  loadHomeData() {
    this.isLoading = true;
    let completed = 0;
    const total = 4;

    const checkDone = () => {
      completed++;
      if (completed >= total) {
        this.isLoading = false;
      }
    };

    return {
      add: (fn: () => void) => {
        this.fetchBanners();
        this.fetchCategories();
        this.fetchFlashDeals();
        this.fetchFeaturedProducts();
        fn();
      },
    };
  }

  private fetchBanners(): void {
    this.apiService.get('/api/v1/banners', { position: 'home' })
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of({ data: this.getDefaultBanners() })),
      )
      .subscribe({
        next: (res: any) => {
          this.banners = res?.data?.length ? res.data : this.getDefaultBanners();
        },
        error: () => {
          this.banners = this.getDefaultBanners();
        },
      });
  }

  private fetchCategories(): void {
    this.apiService.get('/api/v1/categories', { limit: 12, active: true })
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of({ data: [] })),
      )
      .subscribe({
        next: (res: any) => {
          this.categories = res?.data || [];
        },
        error: () => {
          this.categories = [];
        },
      });
  }

  private fetchFlashDeals(): void {
    this.apiService.get('/api/v1/products', { flash_deal: true, limit: 10 })
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of({ data: [] })),
      )
      .subscribe({
        next: (res: any) => {
          this.flashDeals = res?.data || [];
        },
        error: () => {
          this.flashDeals = [];
        },
      });
  }

  private fetchFeaturedProducts(): void {
    this.apiService.get('/api/v1/products', { featured: true, limit: 6 })
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of({ data: [] })),
      )
      .subscribe({
        next: (res: any) => {
          this.featuredProducts = res?.data || [];
        },
        error: () => {
          this.featuredProducts = [];
        },
      });
  }

  private fetchRecommendedProducts(): void {
    this.apiService.get('/api/v1/products', { recommended: true, limit: 10 })
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of({ data: [] })),
      )
      .subscribe({
        next: (res: any) => {
          this.recommendedProducts = res?.data || [];
        },
        error: () => {
          this.recommendedProducts = [];
        },
      });
  }

  private getDefaultBanners(): any[] {
    return [
      {
        id: 1,
        title: 'Summer Collection',
        subtitle: 'Up to 60% off',
        image: null,
        gradient: 'linear-gradient(135deg, #6366f1 0%, #8b5cf6 50%, #a855f7 100%)',
      },
      {
        id: 2,
        title: 'New Arrivals',
        subtitle: 'Fresh styles every day',
        image: null,
        gradient: 'linear-gradient(135deg, #0ea5e9 0%, #6366f1 50%, #8b5cf6 100%)',
      },
      {
        id: 3,
        title: 'Flash Sale',
        subtitle: 'Limited time only',
        image: null,
        gradient: 'linear-gradient(135deg, #f43f5e 0%, #e11d48 50%, #be123c 100%)',
      },
    ];
  }

  private startBannerAutoplay(): void {
    interval(4000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (this.banners.length > 0) {
          this.bannerIndex = (this.bannerIndex + 1) % this.banners.length;
          this.scrollToBanner(this.bannerIndex);
        }
      });
  }

  private startCountdown(): void {
    interval(1000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (
          this.flashDealTimeLeft.seconds === 0 &&
          this.flashDealTimeLeft.minutes === 0 &&
          this.flashDealTimeLeft.hours === 0
        ) {
          return;
        }
        if (this.flashDealTimeLeft.seconds > 0) {
          this.flashDealTimeLeft.seconds--;
        } else if (this.flashDealTimeLeft.minutes > 0) {
          this.flashDealTimeLeft.minutes--;
          this.flashDealTimeLeft.seconds = 59;
        } else if (this.flashDealTimeLeft.hours > 0) {
          this.flashDealTimeLeft.hours--;
          this.flashDealTimeLeft.minutes = 59;
          this.flashDealTimeLeft.seconds = 59;
        }
      });
  }

  private scrollToBanner(index: number): void {
    if (this.bannerSlider?.nativeElement) {
      const slider = this.bannerSlider.nativeElement;
      const slideWidth = slider.offsetWidth;
      slider.scrollTo({
        left: slideWidth * index,
        behavior: 'smooth',
      });
    }
  }

  onBannerScroll(event: any): void {
    const scrollLeft = event.target.scrollLeft;
    const slideWidth = event.target.offsetWidth;
    this.bannerIndex = Math.round(scrollLeft / slideWidth);
  }

  onScroll(_event: any): void {
  }

  navigateToSearch(): void {
    this.router.navigate(['/tabs/search']);
  }

  navigateToCategory(category: Category): void {
    this.router.navigate(['/tabs/search'], {
      queryParams: { category: category.id, name: category.name },
    });
  }

  navigateToProduct(product: Product): void {
    this.router.navigate(['/product', product.id]);
  }

  navigateToFlashDeals(): void {
    this.router.navigate(['/tabs/search'], {
      queryParams: { flash_deal: true },
    });
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
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(price);
  }

  trackByBanner(index: number, item: any): number {
    return item.id || index;
  }

  trackByCategory(index: number, item: Category): number {
    return item.id || index;
  }

  trackByProduct(index: number, item: Product): number {
    return item.id || index;
  }
}
