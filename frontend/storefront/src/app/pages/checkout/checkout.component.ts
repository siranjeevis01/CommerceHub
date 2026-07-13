import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '@shared/services/api.service';
import { CartService } from '@shared/services/cart.service';
import { Cart, Order } from '@shared/models';
import { Observable, of } from 'rxjs';
import { take, switchMap } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-checkout',
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CheckoutComponent {
  cart$: Observable<Cart> = this.cartService.cart$;
  currentStep = 1;
  isSubmitting = false;
  selectedPayment = 'credit_card';
  upiQrData: string | null = null;
  upiUri: string | null = null;
  phoneNumber = '';

  shippingForm: FormGroup;
  paymentForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private cartService: CartService,
    private api: ApiService,
    private toastr: ToastrService,
  ) {
    this.shippingForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      street: ['', Validators.required],
      city: ['', Validators.required],
      state: ['', Validators.required],
      zipCode: ['', [Validators.required, Validators.pattern(/^\d{5}(-\d{4})?$/)]],
      country: ['US', Validators.required],
      phone: ['', Validators.pattern(/^[\d\s\-()+]{7,15}$/)],
    });

    this.paymentForm = this.fb.group({
      method: ['credit_card', Validators.required],
      cardName: [''],
      cardNumber: [''],
      cardExpiry: [''],
      cardCvv: [''],
      phoneNumber: [''],
    });
  }

  nextStep(): void {
    if (this.currentStep === 1 && this.shippingForm.invalid) {
      this.markFormTouched(this.shippingForm);
      this.toastr.error('Please fill in all required shipping fields');
      return;
    }
    if (this.currentStep === 1) this.currentStep = 2;
    else if (this.currentStep === 2) {
      if (this.selectedPayment === 'credit_card' && (!this.paymentForm.value.cardNumber || !this.paymentForm.value.cardExpiry || !this.paymentForm.value.cardCvv)) {
        this.toastr.error('Please complete payment details');
        return;
      }
      if ((this.selectedPayment === 'upi' || this.selectedPayment === 'whatsapp') && !this.paymentForm.value.phoneNumber) {
        this.toastr.error('Please enter a phone number');
        return;
      }
      this.currentStep = 3;
    }
  }

  prevStep(): void {
    if (this.currentStep > 1) this.currentStep--;
  }

  placeOrder(): void {
    this.isSubmitting = true;
    this.cart$.pipe(take(1)).subscribe(cart => {
      const order = {
        shippingAddress: this.shippingForm.value,
        paymentMethod: this.selectedPayment,
        items: cart.items,
        subtotal: this.getSubtotal(cart),
        tax: this.getTax(cart),
        shippingCost: this.getShipping(),
        total: this.getTotal(cart),
      };
      this.api.post<Order>('/api/v1/orders', order).pipe(
        switchMap((result: any) => {
          if (this.selectedPayment === 'upi' || this.selectedPayment === 'whatsapp') {
            return this.api.post<any>('/api/v1/payments/upi/qr', {
              orderId: result?.id || 0,
              userId: 0,
              amount: this.getTotal(cart),
              currency: 'INR',
            });
          }
          return of(result);
        })
      ).subscribe({
        next: (result: any) => {
          this.isSubmitting = false;
          if (result?.clientSecret) {
            this.upiQrData = 'data:image/png;base64,' + result.clientSecret;
          }
          if (this.selectedPayment === 'whatsapp' && this.paymentForm.value.phoneNumber) {
            this.sendWhatsAppQr();
          }
          this.toastr.success('Order placed successfully!');
          if (!this.upiQrData) {
            this.router.navigate(['/orders']);
          }
        },
        error: () => {
          this.isSubmitting = false;
          this.toastr.error('Failed to place order. Please try again.');
        },
      });
    });
  }

  sendWhatsAppQr(): void {
    if (!this.upiUri || !this.paymentForm.value.phoneNumber) return;
    this.api.post('/api/v1/payments/whatsapp/send-qr', {
      phoneNumber: this.paymentForm.value.phoneNumber,
      upiUri: this.upiUri,
      amount: 0,
      currency: 'INR',
      orderId: '',
    }).subscribe();
  }

  selectPayment(method: string): void {
    this.selectedPayment = method;
    this.paymentForm.patchValue({ method });
    this.upiQrData = null;
  }

  getSubtotal(cart: Cart | null): number {
    if (!cart) return 0;
    return cart.items.reduce((sum, i) => sum + i.totalPrice, 0);
  }

  getShipping(): number {
    return 0;
  }

  getTax(cart: Cart | null): number {
    if (!cart) return 0;
    return this.getSubtotal(cart) * 0.08;
  }

  getTotal(cart: Cart | null): number {
    if (!cart) return 0;
    return this.getSubtotal(cart) + this.getShipping() + this.getTax(cart);
  }

  private markFormTouched(form: FormGroup): void {
    Object.keys(form.controls).forEach(key => {
      form.controls[key].markAsTouched();
    });
  }
}
