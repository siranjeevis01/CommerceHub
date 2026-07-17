import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject, takeUntil, catchError, of } from 'rxjs';
import { ToastController, AlertController } from '@ionic/angular';

import { ApiService } from '@core/services/api.service';
import { VendorProfile } from '@core/models';

type StatusFilter = 'all' | 'pending' | 'approved' | 'rejected';

@Component({
  standalone: false,
  selector: 'app-vendors',
  templateUrl: './vendors.page.html',
  styleUrls: ['./vendors.page.scss'],
})
export class VendorsPage implements OnInit, OnDestroy {
  vendors: VendorProfile[] = [];
  filteredVendors: VendorProfile[] = [];
  selectedStatus: StatusFilter = 'all';
  isLoading = true;
  isRefreshing = false;
  searchQuery = '';

  statusFilters: { value: StatusFilter; label: string; icon: string; color: string }[] = [
    { value: 'all', label: 'All', icon: 'list-outline', color: '#6366f1' },
    { value: 'pending', label: 'Pending', icon: 'hourglass-outline', color: '#f59e0b' },
    { value: 'approved', label: 'Approved', icon: 'checkmark-circle-outline', color: '#10b981' },
    { value: 'rejected', label: 'Rejected', icon: 'close-circle-outline', color: '#ef4444' },
  ];

  private destroy$ = new Subject<void>();

  constructor(
    private api: ApiService,
    private toastCtrl: ToastController,
    private alertCtrl: AlertController,
  ) {}

  ngOnInit(): void {
    this.loadVendors();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadVendors(): void {
    this.isLoading = true;
    const params: any = {};
    if (this.selectedStatus !== 'all') {
      params.status = this.selectedStatus;
    }

    this.api.get<any>('/api/v1/vendors', params)
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of({ data: [] })),
      )
      .subscribe({
        next: (res: any) => {
          this.vendors = res?.data ?? [];
          this.applyFilters();
          this.isLoading = false;
        },
        error: () => {
          this.vendors = [];
          this.filteredVendors = [];
          this.isLoading = false;
        },
      });
  }

  doRefresh(event: any): void {
    this.isRefreshing = true;
    this.loadVendors();
    event.target?.complete();
    this.isRefreshing = false;
  }

  selectStatus(status: StatusFilter): void {
    this.selectedStatus = status;
    this.loadVendors();
  }

  onSearchInput(event: any): void {
    this.searchQuery = event.detail.value ?? '';
    this.applyFilters();
  }

  private applyFilters(): void {
    let result = [...this.vendors];
    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase();
      result = result.filter(v =>
        v.storeName.toLowerCase().includes(q) ||
        v.email.toLowerCase().includes(q)
      );
    }
    this.filteredVendors = result;
  }

  async approveVendor(vendor: VendorProfile): Promise<void> {
    const alert = await this.alertCtrl.create({
      header: 'Approve Vendor',
      message: `Approve <strong>${vendor.storeName}</strong>? They will be able to list products.`,
      cssClass: 'futuristic-alert',
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Approve',
          cssClass: 'alert-success',
          handler: () => this.performApprove(vendor),
        },
      ],
    });
    alert.present();
  }

  private performApprove(vendor: VendorProfile): void {
    this.api.post(`/api/v1/vendors/${vendor.id}/approve`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: async () => {
          vendor.status = 'approved';
          await this.showToast(`${vendor.storeName} approved successfully`, 'success');
          this.applyFilters();
        },
        error: async () => {
          await this.showToast('Failed to approve vendor', 'danger');
        },
      });
  }

  async rejectVendor(vendor: VendorProfile): Promise<void> {
    let rejectReason = '';
    const alert = await this.alertCtrl.create({
      header: 'Reject Vendor',
      message: `Reject <strong>${vendor.storeName}</strong>?`,
      cssClass: 'futuristic-alert',
      inputs: [
        {
          name: 'reason',
          type: 'text',
          placeholder: 'Reason for rejection (optional)',
        },
      ],
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Reject',
          cssClass: 'alert-danger',
          handler: (data) => {
            rejectReason = data.reason || '';
            this.performReject(vendor, rejectReason);
          },
        },
      ],
    });
    alert.present();
  }

  private performReject(vendor: VendorProfile, reason: string): void {
    this.api.post(`/api/v1/vendors/${vendor.id}/reject`, { reason })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: async () => {
          vendor.status = 'rejected';
          await this.showToast(`${vendor.storeName} rejected`, 'warning');
          this.applyFilters();
        },
        error: async () => {
          await this.showToast('Failed to reject vendor', 'danger');
        },
      });
  }

  getStatusColor(status: string): string {
    switch (status?.toLowerCase()) {
      case 'approved': return '#10b981';
      case 'pending': return '#f59e0b';
      case 'rejected': return '#ef4444';
      default: return '#64748b';
    }
  }

  trackByVendor(index: number, item: VendorProfile): number {
    return item.id;
  }

  trackByFilter(index: number, item: any): string {
    return item.value;
  }

  private async showToast(message: string, color: string): Promise<void> {
    const toast = await this.toastCtrl.create({
      message,
      duration: 2000,
      color,
      position: 'top',
      cssClass: 'futuristic-toast',
    });
    toast.present();
  }
}
