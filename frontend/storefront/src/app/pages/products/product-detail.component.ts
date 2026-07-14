import { Component, OnInit, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@shared/services/api.service';
import { CartService } from '@shared/services/cart.service';
import { Product, Review, ProductVariant } from '@shared/models';
import { Observable, Subject, of } from 'rxjs';
import { map, switchMap, takeUntil, catchError, shareReplay } from 'rxjs/operators';
import { FormControl, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { environment } from '@env/environment';

@Component({
  standalone: false,
  selector: 'app-product-detail',
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductDetailComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  product$!: Observable<Product | null>;
  reviews$!: Observable<Review[]>;
  relatedProducts$!: Observable<Product[]>;
  isLoading = true;
  error = false;

  selectedImage = 0;
  selectedVariant?: ProductVariant;
  quantity = 1;
  activeTab: 'description' | 'reviews' | 'shipping' = 'description';

  quantityControl = new FormControl(1, [Validators.required, Validators.min(1), Validators.max(99)]);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private api: ApiService,
    private cartService: CartService,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    const product$ = this.route.params.pipe(
      switchMap(params => this.api.get<Product>(`/api/v1/products/${params['slug']}`).pipe(
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

    this.product$ = product$;

    product$.subscribe(p => {
      if (p) {
        this.isLoading = false;
        this.selectedImage = 0;
        this.selectedVariant = undefined;
        this.quantity = 1;
      }
    });

    this.reviews$ = product$.pipe(
      switchMap(p => p ? this.api.getPaginated<Review>(`/api/v1/products/${p.id}/reviews`, 1, 10).pipe(map(r => r.data)) : of([])),
    );

    this.relatedProducts$ = product$.pipe(
      switchMap(p => p ? this.api.getPaginated<Product>(`/api/v1/products`, 1, 6).pipe(map(r => r.data)) : of([])),
    );
  }

  selectVariant(variant: ProductVariant): void {
    this.selectedVariant = variant;
    this.quantity = 1;
  }

  get currentPrice(): number {
    return this.selectedVariant?.price ?? 0;
  }

  get stockQuantity(): number {
    return this.selectedVariant?.stockQuantity ?? 0;
  }

  addToCart(): void {
    if (this.stockQuantity === 0) return;
    this.product$.pipe(takeUntil(this.destroy$)).subscribe(product => {
      if (!product) return;
      this.cartService.addItem(
        product.id,
        product.name,
        this.selectedVariant?.imageUrl || product.imageUrl,
        this.currentPrice || product.price,
        this.quantity,
        this.selectedVariant?.id,
      ).subscribe({
        next: () => this.toastr.success('Added to cart'),
        error: () => this.toastr.error('Failed to add to cart'),
      });
    });
  }

  buyNow(): void {
    this.addToCart();
    this.router.navigate(['/checkout']);
  }

  shareOnWhatsApp(): void {
    this.product$.pipe(takeUntil(this.destroy$)).subscribe(product => {
      if (!product) return;
      const price = this.currentPrice || product.price;
      const productUrl = window.location.href;
      const message = `Check out ${product.name} at ₹${price.toFixed(2)}! \n\n${productUrl}\n\nPay via UPI: ${environment.upiId}\n\nvia CommerceHub`;
      const encodedMessage = encodeURIComponent(message);
      const shareUrl = `https://wa.me/?text=${encodedMessage}`;
      window.open(shareUrl, '_blank');
    });
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = 'assets/images/placeholder.svg';
  }

  getCategoryUrl(categoryName: string): string {
    return '/categories/' + categoryName.toLowerCase().replace(/\s+/g, '-');
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
