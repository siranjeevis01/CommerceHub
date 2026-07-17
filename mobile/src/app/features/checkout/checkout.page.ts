import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LoadingController, ToastController, AlertController } from '@ionic/angular';
import { CartService } from '@core/services/cart.service';
import { AuthService } from '@core/services/auth.service';
import { ApiService } from '@core/services/api.service';
import { CartItem, Address, ApiResponse } from '@core/models';

@Component({
  standalone: false,
  selector: 'app-checkout',
  templateUrl: './checkout.page.html',
  styleUrls: ['./checkout.page.scss'],
})
export class CheckoutPage implements OnInit {
  items: CartItem[] = [];
  addresses: Address[] = [];
  selectedAddressId: number | null = null;
  selectedPayment = 'upi';
  subtotal = 0;
  discount = 0;
  shipping = 0;
  tax = 0;
  total = 0;
  isLoading = false;

  paymentMethods = [
    { id: 'upi', label: 'UPI', icon: 'phone-portrait-outline', description: 'Google Pay, PhonePe, Paytm' },
    { id: 'card', label: 'Card', icon: 'card-outline', description: 'Credit or Debit Card' },
    { id: 'cod', label: 'Cash on Delivery', icon: 'cash-outline', description: 'Pay when you receive' },
  ];

  addressForm: FormGroup;
  showAddAddress = false;

  constructor(
    private cartService: CartService,
    private authService: AuthService,
    private api: ApiService,
    private fb: FormBuilder,
    private router: Router,
    private loadingCtrl: LoadingController,
    private toastCtrl: ToastController,
    private alertCtrl: AlertController
  ) {
    this.addressForm = this.fb.group({
      street: ['', Validators.required],
      city: ['', Validators.required],
      state: ['', Validators.required],
      zipCode: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
      country: ['India', Validators.required],
    });
  }

  ngOnInit(): void {
    const cart = this.cartService.cart;
    this.items = cart?.items ?? [];
    this.calculateTotals();
    this.loadAddresses();
  }

  calculateTotals(): void {
    this.subtotal = this.items.reduce((sum, item) => sum + item.totalPrice, 0);
    this.tax = Math.round(this.subtotal * 0.18 * 100) / 100;
    this.total = this.subtotal + this.shipping + this.tax - this.discount;
  }

  loadAddresses(): void {
    this.api.get<Address[]>('/api/v1/addresses').subscribe({
      next: (res) => {
        if (res.success) {
          this.addresses = res.data ?? [];
          const defaultAddr = this.addresses.find(a => a.isDefault);
          if (defaultAddr) {
            this.selectedAddressId = defaultAddr.id;
          } else if (this.addresses.length) {
            this.selectedAddressId = this.addresses[0].id;
          }
        }
      },
      error: () => {
        this.addresses = [];
      },
    });
  }

  selectAddress(id: number): void {
    this.selectedAddressId = id;
  }

  selectPayment(method: string): void {
    this.selectedPayment = method;
  }

  toggleAddAddress(): void {
    this.showAddAddress = !this.showAddAddress;
    if (this.showAddAddress) {
      this.addressForm.reset({ country: 'India' });
    }
  }

  saveAddress(): void {
    if (this.addressForm.invalid) return;

    this.api.post<Address>('/api/v1/addresses', this.addressForm.value).subscribe({
      next: (res) => {
        if (res.success) {
          this.addresses.push(res.data);
          this.selectedAddressId = res.data.id;
          this.showAddAddress = false;
          this.showToast('Address added', 'success');
        }
      },
      error: () => this.showToast('Failed to add address', 'danger'),
    });
  }

  async placeOrder(): Promise<void> {
    if (!this.selectedAddressId) {
      this.showToast('Please select a delivery address', 'warning');
      return;
    }

    const alert = await this.alertCtrl.create({
      header: 'Confirm Order',
      message: `Place order for <strong>{{ total | currency }}</strong>?`,
      cssClass: 'futuristic-alert',
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Place Order',
          handler: () => this.confirmPlaceOrder(),
        },
      ],
    });
    await alert.present();
  }

  private async confirmPlaceOrder(): Promise<void> {
    this.isLoading = true;
    const loading = await this.loadingCtrl.create({
      message: 'Processing your order...',
      cssClass: 'futuristic-loader',
    });
    await loading.present();

    const orderPayload = {
      addressId: this.selectedAddressId,
      paymentMethod: this.selectedPayment,
    };

    this.api.post<{ id: number; orderNumber: string }>('/api/v1/orders', orderPayload).subscribe({
      next: async (res) => {
        await loading.dismiss();
        this.isLoading = false;
        if (res.success) {
          this.cartService.clearCart().subscribe();
          this.router.navigate(['/order', res.data.id], {
            queryParams: { success: 'true' },
          });
          this.showToast('Order placed successfully!', 'success');
        } else {
          this.showToast(res.message || 'Failed to place order', 'danger');
        }
      },
      error: async () => {
        await loading.dismiss();
        this.isLoading = false;
        this.showToast('Something went wrong. Please try again.', 'danger');
      },
    });
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
}
