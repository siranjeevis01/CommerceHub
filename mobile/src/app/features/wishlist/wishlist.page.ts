import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { LoadingController, ToastController, AlertController } from '@ionic/angular';
import { ApiService } from '@core/services/api.service';
import { CartService } from '@core/services/cart.service';
import { Product } from '@core/models';

@Component({
  standalone: false,
  selector: 'app-wishlist',
  templateUrl: './wishlist.page.html',
  styleUrls: ['./wishlist.page.scss'],
})
export class WishlistPage implements OnInit {
  products: Product[] = [];
  isLoading = false;

  constructor(
    private api: ApiService,
    private cartService: CartService,
    private router: Router,
    private loadingCtrl: LoadingController,
    private toastCtrl: ToastController,
    private alertCtrl: AlertController
  ) {}

  ngOnInit(): void {
    this.loadWishlist();
  }

  loadWishlist(event?: any): void {
    this.isLoading = true;
    this.api.get<Product[]>('/api/v1/wishlist').subscribe({
      next: (res) => {
        this.isLoading = false;
        if (event) event.target.complete();
        if (res.success) {
          this.products = res.data ?? [];
        }
      },
      error: () => {
        this.isLoading = false;
        if (event) event.target.complete();
      },
    });
  }

  viewProduct(product: Product): void {
    this.router.navigate(['/product', product.id]);
  }

  addToCart(product: Product): void {
    this.cartService.addToCart(product.id, 1).subscribe({
      next: async (res) => {
        if (res.success) {
          const toast = await this.toastCtrl.create({
            message: `${product.name} added to cart`,
            duration: 2000,
            color: 'success',
            position: 'bottom',
            cssClass: 'futuristic-toast',
            buttons: [{ text: 'View Cart', handler: () => this.router.navigate(['/tabs/cart']) }],
          });
          toast.present();
        }
      },
      error: async () => {
        const toast = await this.toastCtrl.create({
          message: 'Failed to add to cart',
          duration: 2000,
          color: 'danger',
        });
        toast.present();
      },
    });
  }

  async removeFromWishlist(product: Product): Promise<void> {
    const alert = await this.alertCtrl.create({
      header: 'Remove from Wishlist',
      message: `Remove "${product.name}" from your wishlist?`,
      cssClass: 'futuristic-alert',
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Remove',
          role: 'destructive',
          handler: () => {
            this.api.delete(`/api/v1/wishlist/${product.id}`).subscribe({
              next: async (res) => {
                if (res.success) {
                  this.products = this.products.filter(p => p.id !== product.id);
                  const toast = await this.toastCtrl.create({
                    message: 'Removed from wishlist',
                    duration: 2000,
                    color: 'primary',
                  });
                  toast.present();
                }
              },
              error: async () => {
                const toast = await this.toastCtrl.create({
                  message: 'Failed to remove',
                  duration: 2000,
                  color: 'danger',
                });
                toast.present();
              },
            });
          },
        },
      ],
    });
    await alert.present();
  }

  getDiscountPercent(product: Product): number {
    if (!product.comparePrice || product.comparePrice <= product.price) return 0;
    return Math.round(((product.comparePrice - product.price) / product.comparePrice) * 100);
  }

  trackByProductId(_index: number, product: Product): number {
    return product.id;
  }
}
