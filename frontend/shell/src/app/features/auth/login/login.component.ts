import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '@shared/services/auth.service';

@Component({
  standalone: false,
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginComponent {
  loginForm: FormGroup;
  loading = false;
  submitted = false;
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });

    if (this.auth.isLoggedIn) {
      this.router.navigate(['/']);
    }
  }

  get f() {
    return this.loginForm.controls;
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.loginForm.invalid) return;

    this.loading = true;
    const { email, password } = this.loginForm.value;

    this.auth.login({ email, password }).subscribe({
      next: () => {
        this.toastr.success('Welcome back!', 'Login Successful');
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.toastr.error(err.message || 'Invalid email or password', 'Login Failed');
        this.loading = false;
      }
    });
  }

  loginWithGoogle(): void {
    window.location.href = `${(window as any)['env']?.apiUrl || 'http://localhost:5000'}/api/v1/auth/google`;
  }

  loginWithFacebook(): void {
    window.location.href = `${(window as any)['env']?.apiUrl || 'http://localhost:5000'}/api/v1/auth/facebook`;
  }
}
