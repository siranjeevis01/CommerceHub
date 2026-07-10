import { ChangeDetectionStrategy, Component, HostListener } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';

interface NavItem {
  label: string;
  icon: string;
  path?: string;
  badge?: number;
  children?: NavItem[];
  roles?: string[];
}

@Component({
  standalone: false,
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidebarComponent {
  collapsed = false;
  mobileOpen = false;
  currentPath = '';

  navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'bi bi-speedometer2', path: '/dashboard' },
    { label: 'Users', icon: 'bi bi-people', path: '/users' },
    { label: 'Vendors', icon: 'bi bi-shop', path: '/vendors', badge: 3 },
    { label: 'Products', icon: 'bi bi-box-seam', path: '/products',
      children: [
        { label: 'All Products', icon: 'bi bi-list', path: '/products' },
        { label: 'Add Product', icon: 'bi bi-plus-circle', path: '/products/create' },
      ]
    },
    { label: 'Orders', icon: 'bi bi-cart', path: '/orders', badge: 12 },
    { label: 'Categories', icon: 'bi bi-collection', path: '/categories' },
    { label: 'Brands', icon: 'bi bi-tags', path: '/brands' },
    { label: 'Coupons', icon: 'bi bi-percent', path: '/coupons' },
    { label: 'CMS', icon: 'bi bi-file-text', path: '/cms' },
    { label: 'Analytics', icon: 'bi bi-graph-up-arrow', path: '/analytics', roles: ['Admin'] },
    { label: 'Payouts', icon: 'bi bi-wallet2', path: '/payouts', roles: ['Admin'] },
    { label: 'Settings', icon: 'bi bi-gear', path: '/settings' },
  ];

  expandedMenus: Set<string> = new Set();

  constructor(private router: Router) {
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe(() => {
      this.currentPath = this.router.url;
    });
  }

  isActive(path: string): boolean {
    return this.currentPath.startsWith(path);
  }

  isExactActive(path: string): boolean {
    return this.currentPath === path;
  }

  toggleSubMenu(label: string): void {
    if (this.expandedMenus.has(label)) {
      this.expandedMenus.delete(label);
    } else {
      this.expandedMenus.add(label);
    }
  }

  isExpanded(label: string): boolean {
    return this.expandedMenus.has(label);
  }

  toggleCollapse(): void {
    this.collapsed = !this.collapsed;
  }

  @HostListener('window:resize', ['$event'])
  onResize(): void {
    if (window.innerWidth <= 768) {
      this.collapsed = true;
    }
  }
}
