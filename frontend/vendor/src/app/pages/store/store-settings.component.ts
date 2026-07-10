import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { VendorProfile } from '@shared/models';
import { VendorService } from '../../services/vendor.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-store-settings',
  templateUrl: './store-settings.component.html',
  styleUrls: ['./store-settings.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StoreSettingsComponent implements OnInit {
  storeForm: FormGroup;
  loading = true;
  saving = false;

  constructor(
    private fb: FormBuilder,
    private vendorService: VendorService,
    private toastr: ToastrService,
  ) {
    this.storeForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadStoreProfile();
  }

  private createForm(): FormGroup {
    return this.fb.group({
      storeName: ['', Validators.required],
      storeSlug: ['', Validators.required],
      description: [''],
      logoUrl: [''],
      bannerUrl: [''],
      email: ['', [Validators.email]],
      phone: [''],
      address: [''],
      facebook: [''],
      instagram: [''],
      twitter: [''],
    });
  }

  private loadStoreProfile(): void {
    this.vendorService.getStoreProfile().subscribe({
      next: (res) => {
        const p = res.data;
        this.storeForm.patchValue({
          storeName: p.storeName,
          storeSlug: p.storeSlug,
          description: p.description,
          logoUrl: p.logoUrl,
          bannerUrl: p.bannerUrl,
          email: p.email,
          phone: p.phone,
          address: p.address,
        });
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load store profile');
      },
    });
  }

  onSubmit(): void {
    if (this.storeForm.invalid) {
      Object.keys(this.storeForm.controls).forEach(key => {
        if (this.storeForm.get(key)?.invalid) this.storeForm.get(key)?.markAsTouched();
      });
      this.toastr.warning('Please fill required fields');
      return;
    }

    this.saving = true;
    this.vendorService.updateStoreProfile(this.storeForm.value).subscribe({
      next: () => {
        this.saving = false;
        this.toastr.success('Store settings saved');
      },
      error: (err) => {
        this.saving = false;
        this.toastr.error(err.error?.message || 'Failed to save settings');
      },
    });
  }
}
