import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, map, of, switchMap, tap } from 'rxjs';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { Cart, CartItem } from '../models';

@Injectable({ providedIn: 'root' })
export class CartService {
  private cartSubject = new BehaviorSubject<Cart>({ id: '', items: [], total: 0, itemCount: 0, createdAt: new Date().toISOString() });
  cart$: Observable<Cart> = this.cartSubject.asObservable();
  private sessionId = this.getOrCreateSessionId();

  constructor(private api: ApiService, private auth: AuthService) {
    this.loadCart();
  }

  getCart(): Observable<Cart> {
    return this.api.get<Cart>(this.getCartEndpoint()).pipe(
      map(r => r.data),
      tap(cart => {
        cart.total = cart.items.reduce((sum, i) => sum + i.totalPrice, 0);
        cart.itemCount = cart.items.reduce((sum, i) => sum + i.quantity, 0);
        this.cartSubject.next(cart);
      })
    );
  }

  addItem(productId: number, name: string, imageUrl: string, unitPrice: number, quantity: number, variantId?: number): Observable<Cart> {
    return this.api.post<Cart>(`${this.getCartEndpoint()}/add`, { userId: this.auth.currentUser?.id, sessionId: this.sessionId, productId, variantId, name, imageUrl, unitPrice, quantity }).pipe(
      map(r => r.data),
      tap(cart => this.cartSubject.next(cart))
    );
  }

  updateItem(productId: number, quantity: number, variantId?: number): Observable<Cart> {
    return this.api.put<Cart>(`${this.getCartEndpoint()}/update`, { userId: this.auth.currentUser?.id, sessionId: this.sessionId, productId, variantId, quantity }).pipe(
      map(r => r.data),
      tap(cart => this.cartSubject.next(cart))
    );
  }

  removeItem(productId: number, variantId?: number): Observable<Cart> {
    const body = {
      userId: this.auth.currentUser?.id,
      sessionId: this.sessionId,
      productId,
      variantId,
    };
    return this.api.delete<Cart>(`${this.getCartEndpoint()}/remove`, body).pipe(
      map(r => r.data),
      tap(cart => this.cartSubject.next(cart))
    );
  }

  clearCart(): Observable<void> {
    return this.api.delete<void>(`${this.getCartEndpoint()}/clear`).pipe(
      map(r => r.data),
      tap(() => this.loadCart())
    );
  }

  mergeCart(): Observable<Cart> {
    if (!this.auth.isLoggedIn) return this.getCart();
    return this.api.post<Cart>('/api/v1/cart/merge', { userId: this.auth.currentUser!.id, sessionId: this.sessionId }).pipe(
      map(r => r.data),
      tap(cart => this.cartSubject.next(cart))
    );
  }

  private loadCart(): void {
    this.api.get<Cart>(this.getCartEndpoint()).pipe(map(r => r.data)).subscribe({
      next: cart => {
        cart.total = cart.items.reduce((sum, i) => sum + i.totalPrice, 0);
        cart.itemCount = cart.items.reduce((sum, i) => sum + i.quantity, 0);
        this.cartSubject.next(cart);
      }
    });
  }

  private getCartEndpoint(): string {
    if (this.auth.isLoggedIn) return `/api/v1/cart?userId=${this.auth.currentUser!.id}`;
    return `/api/v1/cart?sessionId=${this.sessionId}`;
  }

  private getOrCreateSessionId(): string {
    let id = sessionStorage.getItem('cart_session_id');
    if (!id) {
      id = 'sess_' + Math.random().toString(36).substring(2, 15);
      sessionStorage.setItem('cart_session_id', id);
    }
    return id;
  }
}
