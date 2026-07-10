import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CartService } from '@shared/services/cart.service';
import { AuthService } from '@shared/services/auth.service';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Cart } from '@shared/models';
import { Router } from '@angular/router';

@Component({
  standalone: false,
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  cart$: Observable<Cart> = this.cartService.cart$;
  isLoggedIn$: Observable<boolean>;

  constructor(
    public cartService: CartService,
    public authService: AuthService,
    public router: Router,
  ) {
    this.isLoggedIn$ = authService.currentUser$.pipe(map(u => !!u));
  }

  searchQuery = '';
  currentYear = new Date().getFullYear();

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }

  onSearch(): void {
    if (this.searchQuery.trim()) {
      this.router.navigate(['/search'], { queryParams: { q: this.searchQuery.trim() } });
      this.searchQuery = '';
    }
  }
}
