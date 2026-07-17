import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { ToastController } from '@ionic/angular';

import { ThemeService } from '@core/services/theme.service';
import { ThemeName } from '@core/models';

interface PlatformSetting {
  key: string;
  label: string;
  description: string;
  type: 'toggle' | 'input' | 'select';
  value: any;
  options?: { label: string; value: any }[];
  icon: string;
  category: string;
}

@Component({
  standalone: false,
  selector: 'app-settings',
  templateUrl: './settings.page.html',
  styleUrls: ['./settings.page.scss'],
})
export class SettingsPage implements OnInit, OnDestroy {
  currentTheme: ThemeName = 'dark';
  settings: PlatformSetting[] = [];
  isLoading = false;

  private destroy$ = new Subject<void>();

  constructor(
    public themeService: ThemeService,
    private toastCtrl: ToastController,
  ) {}

  ngOnInit(): void {
    this.currentTheme = this.themeService.currentTheme;
    this.themeService.currentTheme$
      .pipe(takeUntil(this.destroy$))
      .subscribe(theme => this.currentTheme = theme);
    this.initSettings();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initSettings(): void {
    this.settings = [
      {
        key: 'store_name',
        label: 'Store Name',
        description: 'Display name for the marketplace',
        type: 'input',
        value: 'CommerceHub',
        icon: 'storefront-outline',
        category: 'General',
      },
      {
        key: 'store_email',
        label: 'Support Email',
        description: 'Contact email for customer support',
        type: 'input',
        value: 'support@commercehub.com',
        icon: 'mail-outline',
        category: 'General',
      },
      {
        key: 'maintenance_mode',
        label: 'Maintenance Mode',
        description: 'Temporarily disable the storefront',
        type: 'toggle',
        value: false,
        icon: 'construction-outline',
        category: 'General',
      },
      {
        key: 'auto_approve_vendors',
        label: 'Auto-Approve Vendors',
        description: 'Automatically approve new vendor registrations',
        type: 'toggle',
        value: false,
        icon: 'flash-outline',
        category: 'Vendors',
      },
      {
        key: 'vendor_commission',
        label: 'Commission Rate (%)',
        description: 'Platform commission on vendor sales',
        type: 'input',
        value: '10',
        icon: 'percent-outline',
        category: 'Vendors',
      },
      {
        key: 'min_withdrawal',
        label: 'Min. Withdrawal ($)',
        description: 'Minimum amount for vendor payouts',
        type: 'input',
        value: '50',
        icon: 'wallet-outline',
        category: 'Vendors',
      },
      {
        key: 'order_timeout',
        label: 'Order Auto-Cancel (hours)',
        description: 'Cancel unpaid orders after this period',
        type: 'select',
        value: 24,
        options: [
          { label: '12 hours', value: 12 },
          { label: '24 hours', value: 24 },
          { label: '48 hours', value: 48 },
          { label: '72 hours', value: 72 },
        ],
        icon: 'time-outline',
        category: 'Orders',
      },
      {
        key: 'free_shipping_threshold',
        label: 'Free Shipping Threshold ($)',
        description: 'Minimum order for free shipping',
        type: 'input',
        value: '75',
        icon: 'car-outline',
        category: 'Shipping',
      },
      {
        key: 'allow_reviews',
        label: 'Allow Reviews',
        description: 'Enable product reviews from customers',
        type: 'toggle',
        value: true,
        icon: 'chatbubbles-outline',
        category: 'Products',
      },
    ];
  }

  updateSetting(setting: PlatformSetting, value: any): void {
    setting.value = value;
  }

  onInputValue(setting: PlatformSetting, event: any): void {
    setting.value = event.detail.value;
  }

  onSelectValue(setting: PlatformSetting, event: any): void {
    setting.value = Number(event.detail.value);
  }

  saveSettings(): void {
    this.isLoading = true;
    setTimeout(async () => {
      this.isLoading = false;
      await this.showToast('Settings saved successfully', 'success');
    }, 800);
  }

  setTheme(theme: ThemeName): void {
    this.themeService.setTheme(theme);
  }

  get settingsCategories(): string[] {
    return [...new Set(this.settings.map(s => s.category))];
  }

  getSettingsByCategory(category: string): PlatformSetting[] {
    return this.settings.filter(s => s.category === category);
  }

  trackBySetting(index: number, item: PlatformSetting): string {
    return item.key;
  }

  private async showToast(message: string, color: string): Promise<void> {
    const toast = await this.toastCtrl.create({
      message,
      duration: 2000,
      color,
      position: 'top',
      cssClass: 'futuristic-toast',
    });
    toast.present();
  }
}
