import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { ApiService } from '@shared/services/api.service';
import { Address } from '@shared/models';

@Component({
  standalone: false,
  selector: 'app-address-list',
  templateUrl: './address-list.component.html',
  styleUrls: ['./address-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddressListComponent implements OnInit {
  addresses: Address[] = [];
  loading = false;
  showForm = false;
  editingId: number | null = null;
  addressForm: FormGroup;
  submitted = false;
  saving = false;

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private toastr: ToastrService
  ) {
    this.addressForm = this.fb.group({
      street: ['', Validators.required],
      city: ['', Validators.required],
      state: ['', Validators.required],
      zipCode: ['', [Validators.required, Validators.pattern(/^\d{5}(-\d{4})?$/)]],
      country: ['', Validators.required],
      isDefault: [false]
    });
  }

  get f() { return this.addressForm.controls; }

  ngOnInit(): void {
    this.loadAddresses();
  }

  private loadAddresses(): void {
    this.loading = true;
    this.api.get<Address[]>('/api/v1/addresses').subscribe({
      next: (res) => {
        this.addresses = res.data;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  openAddForm(): void {
    this.editingId = null;
    this.addressForm.reset({ isDefault: false });
    this.showForm = true;
    this.submitted = false;
  }

  openEditForm(address: Address): void {
    this.editingId = address.id;
    this.addressForm.patchValue(address);
    this.showForm = true;
    this.submitted = false;
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingId = null;
    this.addressForm.reset();
    this.submitted = false;
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.addressForm.invalid) return;

    this.saving = true;
    const data = this.addressForm.value;

    const request = this.editingId
      ? this.api.put<Address>(`/api/v1/addresses/${this.editingId}`, data)
      : this.api.post<Address>('/api/v1/addresses', data);

    request.subscribe({
      next: () => {
        this.toastr.success(this.editingId ? 'Address updated' : 'Address added', 'Success');
        this.saving = false;
        this.cancelForm();
        this.loadAddresses();
      },
      error: (err) => {
        this.toastr.error(err.message || 'Failed to save address', 'Error');
        this.saving = false;
      }
    });
  }

  deleteAddress(id: number): void {
    if (!confirm('Are you sure you want to delete this address?')) return;

    this.api.delete(`/api/v1/addresses/${id}`).subscribe({
      next: () => {
        this.toastr.success('Address deleted', 'Success');
        this.loadAddresses();
      },
      error: (err) => this.toastr.error(err.message || 'Failed to delete address', 'Error')
    });
  }

  setDefault(id: number): void {
    this.api.put(`/api/v1/addresses/${id}/default`, {}).subscribe({
      next: () => {
        this.toastr.success('Default address updated', 'Success');
        this.loadAddresses();
      },
      error: (err) => this.toastr.error(err.message || 'Failed to set default address', 'Error')
    });
  }
}
