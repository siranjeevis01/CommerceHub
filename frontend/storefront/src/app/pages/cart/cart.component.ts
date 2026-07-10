import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CartService } from '@shared/services/cart.service';
import { Cart, CartItem } from '@shared/models';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CartComponent {
  cart$: Observable<Cart> = this.cartService.cart$;
  couponCode = '';
  estimatedShipping = 5.99;
  taxRate = 0.08;

  constructor(
    private cartService: CartService,
    private toastr: ToastrService,
  ) {}

  updateQuantity(item: CartItem, quantity: number): void {
    if (quantity < 1) return;
    this.cartService.updateItem(item.productId, quantity, item.variantId).subscribe({
      error: () => this.toastr.error('Failed to update quantity'),
    });
  }

  removeItem(item: CartItem): void {
    this.cartService.removeItem(item.productId, item.variantId).subscribe({
      next: () => this.toastr.success(`${item.name} removed from cart`),
      error: () => this.toastr.error('Failed to remove item'),
    });
  }

  clearCart(): void {
    this.cartService.clearCart().subscribe({
      next: () => this.toastr.success('Cart cleared'),
      error: () => this.toastr.error('Failed to clear cart'),
    });
  }

  applyCoupon(): void {
    if (this.couponCode.trim()) {
      this.toastr.info('Coupon applied (demo)');
    }
  }

  getSubtotal(cart: Cart): number {
    return cart.items.reduce((sum, item) => sum + item.totalPrice, 0);
  }

  getShipping(cart: Cart): number {
    const subtotal = this.getSubtotal(cart);
    return subtotal >= 50 ? 0 : this.estimatedShipping;
  }

  getTax(cart: Cart): number {
    return this.getSubtotal(cart) * this.taxRate;
  }

  getTotal(cart: Cart): number {
    return this.getSubtotal(cart) + this.getShipping(cart) + this.getTax(cart);
  }
}
