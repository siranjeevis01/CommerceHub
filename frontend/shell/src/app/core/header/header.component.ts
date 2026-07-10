import { Component, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, map } from 'rxjs';
import { AuthService } from '@shared/services/auth.service';
import { CartService } from '@shared/services/cart.service';
import { NotificationService } from '@shared/services/notification.service';
import { User, Notification } from '@shared/models';

@Component({
  standalone: false,
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HeaderComponent {
  currentUser$: Observable<User | null>;
  cartItemCount$: Observable<number>;
  notifications$: Observable<Notification[]>;
  unreadCount$: Observable<number>;
  searchQuery = '';
  isNavCollapsed = true;
  showNotifications = false;

  constructor(
    public auth: AuthService,
    private cart: CartService,
    private notification: NotificationService,
    private router: Router
  ) {
    this.currentUser$ = this.auth.currentUser$;
    this.cartItemCount$ = this.cart.cart$.pipe(map(c => c.itemCount));
    this.notifications$ = this.notification.notifications$;
    this.unreadCount$ = this.notification.unreadCount$;
  }

  onSearch(): void {
    if (this.searchQuery.trim()) {
      this.router.navigate(['/storefront/search'], { queryParams: { q: this.searchQuery.trim() } });
      this.searchQuery = '';
    }
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/']);
  }

  toggleNotifications(): void {
    this.showNotifications = !this.showNotifications;
    if (this.showNotifications) {
      this.notification.markAllAsRead();
    }
  }

  closeNotifications(): void {
    this.showNotifications = false;
  }

  toggleNav(): void {
    this.isNavCollapsed = !this.isNavCollapsed;
  }
}
