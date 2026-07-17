import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Cart, CartItem, ApiResponse } from '../models';
import { BehaviorSubject, Observable, tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class CartService {
  private cartSubject = new BehaviorSubject<Cart | null>(null);
  cart$: Observable<Cart | null> = this.cartSubject.asObservable();

  constructor(private api: ApiService) {}

  get cart(): Cart | null {
    return this.cartSubject.value;
  }

  get itemCount(): number {
    return this.cart?.itemCount ?? 0;
  }

  get total(): number {
    return this.cart?.total ?? 0;
  }

  loadCart(): Observable<ApiResponse<Cart>> {
    return this.api.get<Cart>('/api/v1/cart').pipe(
      tap(response => {
        if (response.success) {
          this.cartSubject.next(response.data);
        }
      })
    );
  }

  addToCart(productId: number, quantity: number = 1, variantId?: number): Observable<ApiResponse<Cart>> {
    return this.api.post<Cart>('/api/v1/cart/items', { productId, quantity, variantId }).pipe(
      tap(response => {
        if (response.success) {
          this.cartSubject.next(response.data);
        }
      })
    );
  }

  updateQuantity(productId: number, quantity: number): Observable<ApiResponse<Cart>> {
    return this.api.put<Cart>(`/api/v1/cart/items/${productId}`, { quantity }).pipe(
      tap(response => {
        if (response.success) {
          this.cartSubject.next(response.data);
        }
      })
    );
  }

  removeItem(productId: number): Observable<ApiResponse<Cart>> {
    return this.api.delete<Cart>(`/api/v1/cart/items/${productId}`).pipe(
      tap(response => {
        if (response.success) {
          this.cartSubject.next(response.data);
        }
      })
    );
  }

  applyCoupon(code: string): Observable<ApiResponse<Cart>> {
    return this.api.post<Cart>('/api/v1/cart/coupon', { code }).pipe(
      tap(response => {
        if (response.success) {
          this.cartSubject.next(response.data);
        }
      })
    );
  }

  clearCart(): Observable<ApiResponse<void>> {
    return this.api.delete<void>('/api/v1/cart').pipe(
      tap(() => this.cartSubject.next(null))
    );
  }
}
