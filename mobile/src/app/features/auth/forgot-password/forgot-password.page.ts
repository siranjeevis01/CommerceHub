import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LoadingController, ToastController } from '@ionic/angular';
import { Subject, takeUntil } from 'rxjs';

import { AuthService } from '@core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.page.html',
  styleUrls: ['./forgot-password.page.scss'],
  standalone: false,
})
export class ForgotPasswordPage implements OnInit, OnDestroy {
  forgotForm!: FormGroup;
  isSubmitting = false;
  emailSent = false;
  sentEmail = '';

  private destroy$ = new Subject<void>();

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly loadingCtrl: LoadingController,
    private readonly toastCtrl: ToastController
  ) {}

  ngOnInit(): void {
    this.initForm();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initForm(): void {
    this.forgotForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  get f() {
    return this.forgotForm.controls;
  }

  async onSubmit(): Promise<void> {
    if (this.forgotForm.invalid) {
      this.forgotForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const loading = await this.loadingCtrl.create({
      message: 'Sending reset link...',
      spinner: 'crescent',
      cssClass: 'futuristic-loader',
    });

    try {
      await loading.present();

      const email = this.f['email'].value;

      // Simulate API call for password reset
      await new Promise((resolve) => setTimeout(resolve, 1500));

      this.sentEmail = email;
      this.emailSent = true;
      await loading.dismiss();
      await this.showToast(
        'Password reset link sent! Check your inbox.',
        'success'
      );
    } catch {
      await loading.dismiss();
      await this.showToast(
        'Failed to send reset link. Please try again.',
        'danger'
      );
    } finally {
      this.isSubmitting = false;
    }
  }

  resendEmail(): void {
    this.emailSent = false;
    this.onSubmit();
  }

  navigateToLogin(): void {
    this.router.navigate(['/auth/login']);
  }

  private async showToast(
    message: string,
    color: string
  ): Promise<void> {
    const toast = await this.toastCtrl.create({
      message,
      duration: 3000,
      color,
      position: 'top',
      cssClass: 'futuristic-toast',
    });
    await toast.present();
  }
}
