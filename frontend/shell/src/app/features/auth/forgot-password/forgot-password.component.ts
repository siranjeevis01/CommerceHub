import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { ApiService } from '@shared/services/api.service';

@Component({
  standalone: false,
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ForgotPasswordComponent {
  forgotForm: FormGroup;
  loading = false;
  submitted = false;
  emailSent = false;

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private toastr: ToastrService
  ) {
    this.forgotForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  get f() {
    return this.forgotForm.controls;
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.forgotForm.invalid) return;

    this.loading = true;
    const { email } = this.forgotForm.value;

    this.api.post('/api/v1/auth/forgot-password', { email }).subscribe({
      next: () => {
        this.emailSent = true;
        this.toastr.success('Password reset link has been sent to your email', 'Check Your Email');
        this.loading = false;
      },
      error: (err) => {
        this.toastr.error(err.message || 'Failed to send reset email', 'Error');
        this.loading = false;
      }
    });
  }
}
