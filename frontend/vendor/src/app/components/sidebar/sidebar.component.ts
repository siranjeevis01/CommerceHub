import { ChangeDetectionStrategy, Component } from '@angular/core';

interface NavItem {
  label: string;
  route: string;
  icon: string;
  badge?: number;
}

@Component({
  standalone: false,
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidebarComponent {
  navItems: NavItem[] = [
    { label: 'Dashboard', route: '/dashboard', icon: 'grid-fill' },
    { label: 'Products', route: '/products', icon: 'box-seam' },
    { label: 'Orders', route: '/orders', icon: 'cart-check' },
    { label: 'Payouts', route: '/payouts', icon: 'wallet2' },
    { label: 'Commissions', route: '/commissions', icon: 'percent' },
    { label: 'Reviews', route: '/reviews', icon: 'star' },
    { label: 'Store Settings', route: '/store', icon: 'gear' },
    { label: 'Analytics', route: '/analytics', icon: 'bar-chart-line' },
  ];
}
