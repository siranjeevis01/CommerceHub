import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AlertController, ToastController } from '@ionic/angular';
import { AuthService } from '@core/services/auth.service';
import { ThemeService } from '@core/services/theme.service';
import { User, ThemeName } from '@core/models';

@Component({
  standalone: false,
  selector: 'app-profile',
  templateUrl: './profile.page.html',
  styleUrls: ['./profile.page.scss'],
})
export class ProfilePage implements OnInit {
  user: User | null = null;
  themes: { name: ThemeName; label: string; icon: string }[] = [];
  currentTheme: ThemeName = 'dark';

  menuItems = [
    { icon: 'receipt-outline', label: 'My Orders', route: '/tabs/orders', color: '#6366f1' },
    { icon: 'location-outline', label: 'Addresses', route: null, color: '#0ea5e9' },
    { icon: 'settings-outline', label: 'Settings', route: null, color: '#8b5cf6' },
    { icon: 'help-circle-outline', label: 'Help & Support', route: null, color: '#10b981' },
    { icon: 'information-circle-outline', label: 'About', route: null, color: '#f59e0b' },
  ];

  constructor(
    private authService: AuthService,
    private themeService: ThemeService,
    private router: Router,
    private alertCtrl: AlertController,
    private toastCtrl: ToastController
  ) {}

  ngOnInit(): void {
    this.themes = this.themeService.availableThemes;
    this.currentTheme = this.themeService.currentTheme;
    this.authService.currentUser$.subscribe(user => {
      this.user = user;
    });
  }

  get initials(): string {
    if (!this.user) return '?';
    const first = this.user.firstName?.[0] ?? '';
    const last = this.user.lastName?.[0] ?? '';
    return (first + last).toUpperCase();
  }

  get fullName(): string {
    if (!this.user) return 'Guest';
    return `${this.user.firstName} ${this.user.lastName}`;
  }

  navigateTo(route: string | null): void {
    if (route) {
      this.router.navigate([route]);
    }
  }

  selectTheme(theme: ThemeName): void {
    this.themeService.setTheme(theme);
    this.currentTheme = theme;
    this.showToast(`Theme changed to ${theme}`);
  }

  async logout(): Promise<void> {
    const alert = await this.alertCtrl.create({
      header: 'Logout',
      message: 'Are you sure you want to logout?',
      cssClass: 'futuristic-alert',
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Logout',
          role: 'destructive',
          handler: async () => {
            await this.authService.logout();
            this.showToast('Logged out successfully');
          },
        },
      ],
    });
    await alert.present();
  }

  private async showToast(message: string): Promise<void> {
    const toast = await this.toastCtrl.create({
      message,
      duration: 2000,
      color: 'primary',
      position: 'bottom',
      cssClass: 'futuristic-toast',
    });
    toast.present();
  }
}
