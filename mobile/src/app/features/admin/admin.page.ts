import { Component } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-admin',
  templateUrl: './admin.page.html',
  styleUrls: ['./admin.page.scss'],
})
export class AdminPage {
  tabs = [
    { label: 'Dashboard', icon: 'speedometer-outline', path: '/admin/dashboard' },
    { label: 'Users', icon: 'people-outline', path: '/admin/users' },
    { label: 'Vendors', icon: 'storefront-outline', path: '/admin/vendors' },
    { label: 'Settings', icon: 'settings-outline', path: '/admin/settings' },
  ];

  getSelectedTab(path: string): boolean {
    return window.location.pathname.includes(path);
  }
}
