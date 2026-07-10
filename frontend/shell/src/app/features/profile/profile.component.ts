import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '@shared/services/auth.service';
import { ApiService } from '@shared/services/api.service';
import { User } from '@shared/models';

@Component({
  standalone: false,
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileComponent implements OnInit {
  profileForm: FormGroup;
  passwordForm: FormGroup;
  user: User | null = null;
  loading = false;
  savingProfile = false;
  changingPassword = false;
  submitted = false;
  passwordSubmitted = false;
  showCurrentPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private api: ApiService,
    private toastr: ToastrService
  ) {
    this.profileForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: [{ value: '', disabled: true }],
      phoneNumber: ['', [Validators.pattern(/^\+?[\d\s\-()]{7,15}$/)]]
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmNewPassword: ['', [Validators.required]]
    });
  }

  get pf() { return this.profileForm.controls; }
  get pwf() { return this.passwordForm.controls; }

  ngOnInit(): void {
    this.loadProfile();
  }

  private loadProfile(): void {
    this.loading = true;
    this.user = this.auth.currentUser;
    if (this.user) {
      this.profileForm.patchValue({
        firstName: this.user.firstName,
        lastName: this.user.lastName,
        email: this.user.email,
        phoneNumber: this.user.phoneNumber || ''
      });
      this.loading = false;
    } else {
      this.api.get<User>('/api/v1/auth/profile').subscribe({
        next: (res) => {
          this.user = res.data;
          this.profileForm.patchValue({
            firstName: res.data.firstName,
            lastName: res.data.lastName,
            email: res.data.email,
            phoneNumber: res.data.phoneNumber || ''
          });
          this.loading = false;
        },
        error: () => this.loading = false
      });
    }
  }

  onSaveProfile(): void {
    this.submitted = true;
    if (this.profileForm.invalid) return;

    this.savingProfile = true;
    const { firstName, lastName, phoneNumber } = this.profileForm.value;

    this.api.put<User>('/api/v1/auth/profile', { firstName, lastName, phoneNumber }).subscribe({
      next: () => {
        this.toastr.success('Profile updated successfully', 'Saved');
        this.savingProfile = false;
        this.submitted = false;
      },
      error: (err) => {
        this.toastr.error(err.message || 'Failed to update profile', 'Error');
        this.savingProfile = false;
      }
    });
  }

  onChangePassword(): void {
    this.passwordSubmitted = true;
    if (this.passwordForm.invalid) return;
    if (this.passwordForm.value.newPassword !== this.passwordForm.value.confirmNewPassword) {
      this.toastr.error('New passwords do not match', 'Error');
      return;
    }

    this.changingPassword = true;
    const { currentPassword, newPassword } = this.passwordForm.value;

    this.api.put('/api/v1/auth/change-password', { currentPassword, newPassword }).subscribe({
      next: () => {
        this.toastr.success('Password changed successfully', 'Updated');
        this.changingPassword = false;
        this.passwordForm.reset();
        this.passwordSubmitted = false;
      },
      error: (err) => {
        this.toastr.error(err.message || 'Failed to change password', 'Error');
        this.changingPassword = false;
      }
    });
  }
}
