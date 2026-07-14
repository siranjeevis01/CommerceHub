import { Component, OnInit, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { ApiService } from '@shared/services/api.service';
import { CartService } from '@shared/services/cart.service';
import { Product, Category } from '@shared/models';
import { Observable, Subject } from 'rxjs';
import { takeUntil, map } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

interface CarouselSlide {
  image: string;
  title: string;
  subtitle: string;
  link: string;
}

@Component({
  standalone: false,
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  featuredProducts$!: Observable<Product[]>;
  newArrivals$!: Observable<Product[]>;
  categories$!: Observable<Category[]>;
  isLoading = true;
  newsletterEmail = '';
  newsletterSubmitted = false;

  carouselSlides: CarouselSlide[] = [
    { image: 'https://placehold.co/1200x400/4f46e5/ffffff?text=Summer+Sale', title: 'Summer Sale', subtitle: 'Up to 50% off on selected items', link: '/products' },
    { image: 'https://placehold.co/1200x400/0891b2/ffffff?text=New+Collections', title: 'New Collections', subtitle: 'Discover the latest trends', link: '/products' },
    { image: 'https://placehold.co/1200x400/059669/ffffff?text=Free+Shipping', title: 'Free Shipping', subtitle: 'On orders over $50', link: '/products' },
  ];

  constructor(
    private api: ApiService,
    private cartService: CartService,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.featuredProducts$ = this.api.getPaginated<Product>('/api/v1/products', 1, 8).pipe(
      map(r => r.data),
    );
    this.newArrivals$ = this.api.getPaginated<Product>('/api/v1/products', 1, 8).pipe(
      map(r => r.data),
    );
    this.categories$ = this.api.get<Category[]>('/api/v1/categories').pipe(
      map(r => r.data),
    );

    setTimeout(() => this.isLoading = false, 800);
  }

  addToCart(product: Product): void {
    this.cartService.addItem(product.id, product.name, product.imageUrl, product.price, 1).subscribe({
      next: () => this.toastr.success(`${product.name} added to cart`),
      error: () => this.toastr.error('Failed to add item to cart'),
    });
  }

  subscribeNewsletter(): void {
    if (!this.newsletterEmail || !this.newsletterEmail.trim()) {
      return;
    }
    console.log('Newsletter subscription:', this.newsletterEmail);
    this.newsletterSubmitted = true;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
