import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil, catchError, of } from 'rxjs';
import { LoadingController, ToastController, AlertController } from '@ionic/angular';

import { ApiService } from '@core/services/api.service';
import { CartService } from '@core/services/cart.service';
import { AuthService } from '@core/services/auth.service';
import { Product, ProductVariant, Review } from '@core/models';
import { FormControl, Validators } from '@angular/forms';

@Component({
  standalone: false,
  selector: 'app-product-detail',
  templateUrl: './product-detail.page.html',
  styleUrls: ['./product-detail.page.scss'],
})
export class ProductDetailPage implements OnInit, OnDestroy {
  @ViewChild('gallerySlider', { static: false }) gallerySlider!: ElementRef;

  product: Product | null = null;
  reviews: Review[] = [];
  selectedVariant: ProductVariant | null = null;
  selectedColor = '';
  selectedSize = '';
  quantity = 1;
  currentImageIndex = 0;
  isLoading = true;
  isAddingToCart = false;
  isBuyingNow = false;
  showFullDescription = false;
  showReviewForm = false;

  reviewRating = 0;
  reviewTitle = '';
  reviewComment = '';

  private destroy$ = new Subject<void>();
  private productId = 0;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private api: ApiService,
    private cartService: CartService,
    private authService: AuthService,
    private loadingCtrl: LoadingController,
    private toastCtrl: ToastController,
    private alertCtrl: AlertController,
  ) {}

  ngOnInit(): void {
    this.productId = Number(this.route.snapshot.paramMap.get('id'));
    if (this.productId) {
      this.loadProduct();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProduct(): void {
    this.isLoading = true;
    this.api.get<Product>(`/api/v1/products/${this.productId}`)
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of({ data: null })),
      )
      .subscribe({
        next: (res: any) => {
          if (res?.data) {
            this.product = res.data;
            this.initVariants();
            this.loadReviews();
          }
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
        },
      });
  }

  private initVariants(): void {
    if (!this.product?.variants?.length) return;

    const colors = this.getUniqueColors();
    if (colors.length > 0) {
      this.selectedColor = colors[0];
      this.onColorSelect(colors[0]);
    } else if (this.product.variants.length > 0) {
      this.selectedVariant = this.product.variants[0];
    }
  }

  loadReviews(): void {
    this.api.get<any>(`/api/v1/products/${this.productId}/reviews`)
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of({ data: [] })),
      )
      .subscribe({
        next: (res: any) => {
          this.reviews = res?.data ?? [];
        },
      });
  }

  // ─── Image Gallery ──────────────────────────────────────────
  get galleryImages(): string[] {
    if (!this.product) return [];
    const images = [
      this.product.mainImageUrl || this.product.imageUrl,
      ...(this.product.galleryImages ?? []),
      ...(this.product.images ?? []),
    ].filter(Boolean);
    return [...new Set(images)];
  }

  onImageScroll(event: any): void {
    const scrollLeft = event.target.scrollLeft;
    const slideWidth = event.target.offsetWidth;
    this.currentImageIndex = Math.round(scrollLeft / slideWidth);
  }

  scrollToImage(index: number): void {
    if (this.gallerySlider?.nativeElement) {
      const slider = this.gallerySlider.nativeElement;
      const slideWidth = slider.offsetWidth;
      slider.scrollTo({ left: slideWidth * index, behavior: 'smooth' });
    }
  }

  // ─── Variant Selection ──────────────────────────────────────
  getUniqueColors(): string[] {
    if (!this.product?.variants) return [];
    return [...new Set(this.product.variants.filter(v => v.color).map(v => v.color!))];
  }

  getSizesForColor(color: string): string[] {
    if (!this.product?.variants) return [];
    return [...new Set(
      this.product.variants
        .filter(v => v.color === color && v.size)
        .map(v => v.size!)
    )];
  }

  onColorSelect(color: string): void {
    this.selectedColor = color;
    const sizes = this.getSizesForColor(color);
    if (sizes.length > 0) {
      this.selectedSize = sizes[0];
      this.updateSelectedVariant();
    } else {
      this.selectedVariant = this.product?.variants?.find(v => v.color === color) ?? null;
    }
  }

  onSizeSelect(size: string): void {
    this.selectedSize = size;
    this.updateSelectedVariant();
  }

  private updateSelectedVariant(): void {
    if (!this.product?.variants) return;
    this.selectedVariant = this.product.variants.find(
      v => v.color === this.selectedColor && v.size === this.selectedSize
    ) ?? this.product.variants.find(v => v.color === this.selectedColor) ?? null;
  }

  get hasColorVariants(): boolean {
    return this.getUniqueColors().length > 0;
  }

  get hasSizeVariants(): boolean {
    return this.getSizesForColor(this.selectedColor).length > 0;
  }

  // ─── Pricing ────────────────────────────────────────────────
  get currentPrice(): number {
    return this.selectedVariant?.price ?? this.product?.price ?? 0;
  }

  get currentStock(): number {
    return this.selectedVariant?.stockQuantity ?? this.product?.stockQuantity ?? 0;
  }

  get comparePrice(): number | undefined {
    return this.product?.comparePrice;
  }

  get discountPercent(): number {
    const original = this.comparePrice;
    if (original && original > this.currentPrice) {
      return Math.round(((original - this.currentPrice) / original) * 100);
    }
    return 0;
  }

  get inStock(): boolean {
    return this.currentStock > 0;
  }

  // ─── Quantity ───────────────────────────────────────────────
  incrementQuantity(): void {
    if (this.quantity < this.currentStock) {
      this.quantity++;
    }
  }

  decrementQuantity(): void {
    if (this.quantity > 1) {
      this.quantity--;
    }
  }

  // ─── Cart Actions ───────────────────────────────────────────
  async addToCart(): Promise<void> {
    if (!this.product || !this.inStock) return;
    this.isAddingToCart = true;

    this.cartService.addToCart(this.product.id, this.quantity, this.selectedVariant?.id)
      .pipe(
        takeUntil(this.destroy$),
        catchError(async () => {
          await this.showToast('Failed to add to cart', 'danger');
          this.isAddingToCart = false;
          return of(null);
        }),
      )
      .subscribe({
        next: async () => {
          this.isAddingToCart = false;
          await this.showToast('Added to cart', 'success');
        },
      });
  }

  async buyNow(): Promise<void> {
    if (!this.product || !this.inStock) return;
    this.isBuyingNow = true;

    this.cartService.addToCart(this.product.id, this.quantity, this.selectedVariant?.id)
      .pipe(
        takeUntil(this.destroy$),
        catchError(async () => {
          await this.showToast('Failed to add to cart', 'danger');
          this.isBuyingNow = false;
          return of(null);
        }),
      )
      .subscribe({
        next: async () => {
          this.isBuyingNow = false;
          this.router.navigate(['/checkout']);
        },
      });
  }

  // ─── Reviews ────────────────────────────────────────────────
  getRatingArray(rating: number): number[] {
    return Array.from({ length: 5 }, (_, i) => (i < Math.floor(rating) ? 1 : 0));
  }

  toggleReviewForm(): void {
    this.showReviewForm = !this.showReviewForm;
    if (!this.showReviewForm) {
      this.reviewRating = 0;
      this.reviewTitle = '';
      this.reviewComment = '';
    }
  }

  setReviewRating(rating: number): void {
    this.reviewRating = rating;
  }

  async submitReview(): Promise<void> {
    if (!this.authService.isLoggedIn) {
      await this.showToast('Please login to submit a review', 'warning');
      return;
    }
    if (this.reviewRating === 0) {
      await this.showToast('Please select a rating', 'warning');
      return;
    }

    this.api.post(`/api/v1/products/${this.productId}/reviews`, {
      rating: this.reviewRating,
      title: this.reviewTitle,
      comment: this.reviewComment,
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: async () => {
          await this.showToast('Review submitted successfully', 'success');
          this.showReviewForm = false;
          this.reviewRating = 0;
          this.reviewTitle = '';
          this.reviewComment = '';
          this.loadReviews();
        },
        error: async () => {
          await this.showToast('Failed to submit review', 'danger');
        },
      });
  }

  // ─── Share ──────────────────────────────────────────────────
  shareProduct(): void {
    if (navigator.share && this.product) {
      navigator.share({
        title: this.product.name,
        text: this.product.shortDescription,
        url: window.location.href,
      });
    }
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(price);
  }

  getTimeAgo(dateStr: string): string {
    if (!dateStr) return '';
    const now = new Date();
    const date = new Date(dateStr);
    const diffMs = now.getTime() - date.getTime();
    const diffDays = Math.floor(diffMs / 86400000);
    if (diffDays < 1) return 'Today';
    if (diffDays === 1) return 'Yesterday';
    if (diffDays < 30) return `${diffDays}d ago`;
    if (diffDays < 365) return `${Math.floor(diffDays / 30)}mo ago`;
    return `${Math.floor(diffDays / 365)}y ago`;
  }

  trackByImage(index: number, item: string): string {
    return item;
  }

  trackByReview(index: number, item: Review): number {
    return item.id;
  }

  trackByColor(index: number, item: string): string {
    return item;
  }

  trackBySize(index: number, item: string): string {
    return item;
  }

  private async showToast(message: string, color: string): Promise<void> {
    const toast = await this.toastCtrl.create({
      message,
      duration: 2000,
      color,
      position: 'top',
      cssClass: 'futuristic-toast',
    });
    toast.present();
  }
}
