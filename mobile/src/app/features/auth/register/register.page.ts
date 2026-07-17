import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LoadingController, ToastController } from '@ionic/angular';
import { Subject, takeUntil } from 'rxjs';

import { AuthService } from '@core/services/auth.service';
import { ThemeService } from '@core/services/theme.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.page.html',
  styleUrls: ['./register.page.scss'],
  standalone: false,
})
export class RegisterPage implements OnInit, OnDestroy {
  registerForm!: FormGroup;
  showPassword = false;
  showConfirmPassword = false;
  isSubmitting = false;
  currentStep = 1;
  totalSteps = 2;

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
    this.registerForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern(/^\+?[\d\s\-()]{7,15}$/)]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
      acceptTerms: [false, [Validators.requiredTrue]],
    });
  }

  get f() {
    return this.registerForm.controls;
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPassword(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  nextStep(): void {
    if (this.currentStep === 1) {
      if (
        this.f['firstName'].valid &&
        this.f['lastName'].valid &&
        this.f['email'].valid
      ) {
        this.currentStep = 2;
      } else {
        ['firstName', 'lastName', 'email'].forEach((key) =>
          this.f[key].markAsTouched()
        );
      }
    }
  }

  previousStep(): void {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  getStepProgress(): number {
    return (this.currentStep / this.totalSteps) * 100;
  }

  async onSubmit(): Promise<void> {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    if (this.f['password'].value !== this.f['confirmPassword'].value) {
      await this.showToast('Passwords do not match', 'danger');
      return;
    }

    this.isSubmitting = true;
    const loading = await this.loadingCtrl.create({
      message: 'Creating your account...',
      spinner: 'crescent',
      cssClass: 'futuristic-loader',
    });

    try {
      await loading.present();

      const { firstName, lastName, email, phone, password } =
        this.registerForm.value;

      this.authService
        .register({ firstName, lastName, email, phone, password })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: async () => {
            await loading.dismiss();
            await this.showToast(
              'Account created successfully! Please sign in.',
              'success'
            );
            this.router.navigate(['/auth/login']);
          },
          error: async (err) => {
            await loading.dismiss();
            const message =
              err?.error?.message ||
              'Registration failed. Please try again.';
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

  navigateToLogin(): void {
    this.router.navigate(['/auth/login']);
  }

  navigateBack(): void {
    if (this.currentStep > 1) {
      this.previousStep();
    } else {
      this.router.navigate(['/auth/login']);
    }
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
