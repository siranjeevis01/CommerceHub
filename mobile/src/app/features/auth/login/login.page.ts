import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LoadingController, ToastController } from '@ionic/angular';
import { Subject, takeUntil } from 'rxjs';

import { AuthService } from '@core/services/auth.service';
import { ThemeService } from '@core/services/theme.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.page.html',
  styleUrls: ['./login.page.scss'],
  standalone: false,
})
export class LoginPage implements OnInit, OnDestroy {
  loginForm!: FormGroup;
  showPassword = false;
  isSubmitting = false;

  private destroy$ = new Subject<void>();

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly themeService: ThemeService,
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
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false],
    });
  }

  get f() {
    return this.loginForm.controls;
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  async onSubmit(): Promise<void> {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const loading = await this.loadingCtrl.create({
      message: 'Authenticating...',
      spinner: 'crescent',
      cssClass: 'futuristic-loader',
    });

    try {
      await loading.present();

      const { email, password } = this.loginForm.value;

      this.authService
        .login({ email, password })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: async () => {
            await loading.dismiss();
            await this.showToast('Welcome back!', 'success');
            this.router.navigate(['/app/tabs/home']);
          },
          error: async (err) => {
            await loading.dismiss();
            const message =
              err?.error?.message || 'Invalid credentials. Please try again.';
            await this.showToast(message, 'danger');
          },
        });
    } catch {
      await loading.dismiss();
      await this.showToast('An unexpected error occurred.', 'danger');
    } finally {
      this.isSubmitting = false;
    }
  }

  navigateToRegister(): void {
    this.router.navigate(['/auth/register']);
  }

  navigateToForgotPassword(): void {
    this.router.navigate(['/auth/forgot-password']);
  }

  async socialLogin(provider: 'google' | 'apple' | 'facebook'): Promise<void> {
    await this.showToast(`${provider} login coming soon!`, 'medium');
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
