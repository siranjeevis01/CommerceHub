import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LoadingController, ToastController, AlertController } from '@ionic/angular';
import { Subscription } from 'rxjs';
import { CartService } from '@core/services/cart.service';
import { CartItem } from '@core/models';

@Component({
  standalone: false,
  selector: 'app-cart',
  templateUrl: './cart.page.html',
  styleUrls: ['./cart.page.scss'],
})
export class CartPage implements OnInit, OnDestroy {
  items: CartItem[] = [];
  couponForm: FormGroup;
  subtotal = 0;
  discount = 0;
  total = 0;
  private cartSub?: Subscription;

  constructor(
    private cartService: CartService,
    private fb: FormBuilder,
    private router: Router,
    private loadingCtrl: LoadingController,
    private toastCtrl: ToastController,
    private alertCtrl: AlertController
  ) {
    this.couponForm = this.fb.group({
      code: ['', [Validators.required, Validators.minLength(3)]],
    });
  }

  ngOnInit(): void {
    this.cartSub = this.cartService.cart$.subscribe(cart => {
      this.items = cart?.items ?? [];
      this.calculateTotals();
    });
    this.cartService.loadCart().subscribe();
  }

  ngOnDestroy(): void {
    this.cartSub?.unsubscribe();
  }

  calculateTotals(): void {
    this.subtotal = this.items.reduce((sum, item) => sum + item.totalPrice, 0);
    this.total = this.subtotal - this.discount;
  }

  incrementQty(item: CartItem): void {
    const newQty = item.quantity + 1;
    this.cartService.updateQuantity(item.productId, newQty).subscribe();
  }

  decrementQty(item: CartItem): void {
    if (item.quantity <= 1) {
      this.removeItem(item);
      return;
    }
    const newQty = item.quantity - 1;
    this.cartService.updateQuantity(item.productId, newQty).subscribe();
  }

  async removeItem(item: CartItem): Promise<void> {
    const alert = await this.alertCtrl.create({
      header: 'Remove Item',
      message: `Remove "${item.name}" from your cart?`,
      cssClass: 'futuristic-alert',
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Remove',
          role: 'destructive',
          handler: () => {
            this.cartService.removeItem(item.productId).subscribe({
              next: () => this.showToast('Item removed'),
              error: () => this.showToast('Failed to remove item', 'danger'),
            });
          },
        },
      ],
    });
    await alert.present();
  }

  applyCoupon(): void {
    if (this.couponForm.invalid) return;
    const { code } = this.couponForm.value;
    const loading = this.loadingCtrl.create({ message: 'Applying coupon...', cssClass: 'futuristic-loader' });
    loading.then(l => l.present());

    this.cartService.applyCoupon(code).subscribe({
      next: (res) => {
        loading.then(l => l.dismiss());
        if (res.success) {
          this.showToast('Coupon applied!', 'success');
          this.calculateTotals();
        } else {
          this.showToast(res.message || 'Invalid coupon', 'warning');
        }
      },
      error: (err) => {
        loading.then(l => l.dismiss());
        this.showToast('Invalid coupon code', 'danger');
      },
    });
  }

  goToCheckout(): void {
    if (!this.items.length) return;
    this.router.navigate(['/checkout']);
  }

  continueShopping(): void {
    this.router.navigate(['/tabs/home']);
  }

  private async showToast(message: string, color = 'primary'): Promise<void> {
    const toast = await this.toastCtrl.create({
      message,
      duration: 2000,
      color,
      position: 'bottom',
      cssClass: 'futuristic-toast',
    });
    toast.present();
  }

  trackByProductId(_index: number, item: CartItem): number {
    return item.productId;
  }
}
