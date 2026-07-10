import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { ApiService } from '@shared/services/api.service';
import { Product } from '@shared/models';

@Component({
  standalone: false,
  selector: 'app-wishlist',
  templateUrl: './wishlist.component.html',
  styleUrls: ['./wishlist.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WishlistComponent implements OnInit {
  items: Product[] = [];
  loading = false;

  constructor(
    private api: ApiService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadWishlist();
  }

  private loadWishlist(): void {
    this.loading = true;
    this.api.get<Product[]>('/api/v1/wishlist').subscribe({
      next: (res) => {
        this.items = res.data;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  removeFromWishlist(productId: number): void {
    this.api.delete(`/api/v1/wishlist/${productId}`).subscribe({
      next: () => {
        this.items = this.items.filter(i => i.id !== productId);
        this.toastr.success('Removed from wishlist', 'Updated');
      },
      error: (err) => this.toastr.error(err.message || 'Failed to remove item', 'Error')
    });
  }

  addToCart(product: Product): void {
    this.toastr.info('Added to cart', product.name);
  }
}
