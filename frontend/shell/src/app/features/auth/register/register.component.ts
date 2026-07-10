import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '@shared/services/auth.service';

export function passwordStrengthValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value || '';
    const hasUpper = /[A-Z]/.test(value);
    const hasLower = /[a-z]/.test(value);
    const hasNumber = /[0-9]/.test(value);
    const hasSpecial = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(value);
    const valid = hasUpper && hasLower && hasNumber && hasSpecial && value.length >= 8;
    return valid ? null : { weakPassword: true };
  };
}

export function matchPasswordValidator(controlName: string, matchingControlName: string): ValidatorFn {
  return (formGroup: AbstractControl): ValidationErrors | null => {
    const control = formGroup.get(controlName);
    const matchingControl = formGroup.get(matchingControlName);
    if (!control || !matchingControl) return null;
    if (matchingControl.errors && !matchingControl.errors['mustMatch']) return null;
    if (control.value !== matchingControl.value) {
      matchingControl.setErrors({ mustMatch: true });
    } else {
      matchingControl.setErrors(null);
    }
    return null;
  };
}

@Component({
  standalone: false,
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RegisterComponent {
  registerForm: FormGroup;
  loading = false;
  submitted = false;
  showPassword = false;
  showConfirmPassword = false;

  passwordStrength = 0;
  passwordFeedback: string[] = [];

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {
    this.registerForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.pattern(/^\+?[\d\s\-()]{7,15}$/)]],
      password: ['', [Validators.required, Validators.minLength(8), passwordStrengthValidator()]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: matchPasswordValidator('password', 'confirmPassword') });

    this.registerForm.get('password')?.valueChanges.subscribe(value => {
      this.updatePasswordStrength(value);
    });
  }

  get f() {
    return this.registerForm.controls;
  }

  private updatePasswordStrength(password: string): void {
    let score = 0;
    this.passwordFeedback = [];

    if (password.length >= 8) { score += 20; } else { this.passwordFeedback.push('At least 8 characters'); }
    if (/[a-z]/.test(password)) { score += 20; } else { this.passwordFeedback.push('Add lowercase letters'); }
    if (/[A-Z]/.test(password)) { score += 20; } else { this.passwordFeedback.push('Add uppercase letters'); }
    if (/[0-9]/.test(password)) { score += 20; } else { this.passwordFeedback.push('Add numbers'); }
    if (/[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password)) { score += 20; } else { this.passwordFeedback.push('Add special characters'); }

    this.passwordStrength = score;
  }

  getStrengthLabel(): string {
    if (this.passwordStrength < 40) return 'Weak';
    if (this.passwordStrength < 80) return 'Medium';
    return 'Strong';
  }

  getStrengthClass(): string {
    if (this.passwordStrength < 40) return 'bg-danger';
    if (this.passwordStrength < 80) return 'bg-warning';
    return 'bg-success';
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.registerForm.invalid) return;

    this.loading = true;
    const { firstName, lastName, email, phoneNumber, password } = this.registerForm.value;

    this.auth.register({ firstName, lastName, email, phoneNumber, password }).subscribe({
      next: () => {
        this.toastr.success('Your account has been created successfully!', 'Registration Successful');
        this.router.navigate(['/auth/login']);
      },
      error: (err) => {
        this.toastr.error(err.message || 'Registration failed. Please try again.', 'Registration Failed');
        this.loading = false;
      }
    });
  }
}
