import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ApiService } from '@shared/services/api.service';
import { CartService } from '@shared/services/cart.service';
import { Product } from '@shared/models';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, switchMap, tap } from 'rxjs/operators';

@Component({
  standalone: false,
  selector: 'app-wishlist',
  templateUrl: './wishlist.component.html',
  styleUrls: ['./wishlist.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WishlistComponent {
  private refreshTrigger = new BehaviorSubject<void>(undefined);
  wishlistItems$: Observable<Product[]> = this.refreshTrigger.pipe(
    tap(() => this.isLoading = true),
    switchMap(() => this.api.get<Product[]>('/api/v1/wishlist')),
    map(r => r.data),
    tap(() => this.isLoading = false),
  );
  isLoading = true;

  constructor(
    private api: ApiService,
    private cartService: CartService,
    private toastr: ToastrService,
  ) {}

  removeFromWishlist(product: Product): void {
    this.api.delete(`/api/v1/wishlist/${product.id}`).subscribe({
      next: () => {
        this.refreshTrigger.next();
        this.toastr.success(`${product.name} removed from wishlist`);
      },
      error: () => this.toastr.error('Failed to remove item'),
    });
  }

  addToCart(product: Product): void {
    this.cartService.addItem(product.id, product.name, product.imageUrl, product.price, 1).subscribe({
      next: () => this.toastr.success(`${product.name} added to cart`),
      error: () => this.toastr.error('Failed to add item'),
    });
  }
}
