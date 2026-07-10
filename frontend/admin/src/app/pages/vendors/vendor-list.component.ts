import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '@shared/services/api.service';
import { VendorProfile } from '@shared/models';
import { Subject, takeUntil } from 'rxjs';
import { TableColumn, TableAction, FilterOption } from '../../admin.models';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-vendor-list',
  templateUrl: './vendor-list.component.html',
  styleUrls: ['./vendor-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VendorListComponent implements OnInit, OnDestroy {
  vendors: (VendorProfile & { verified?: boolean })[] = [];
  total = 0;
  page = 1;
  pageSize = 10;
  loading = false;
  error: string | null = null;

  private destroy$ = new Subject<void>();

  filters: FilterOption[] = [
    { key: 'search', label: 'Search', type: 'text', placeholder: 'Search store name...' },
    { key: 'status', label: 'Status', type: 'select', placeholder: 'All Status', options: [
      { label: 'Pending', value: 'pending' }, { label: 'Approved', value: 'approved' },
      { label: 'Suspended', value: 'suspended' }, { label: 'Rejected', value: 'rejected' },
    ]},
  ];

  columns: TableColumn[] = [
    { key: 'storeName', label: 'Store Name', sortable: true },
    { key: 'email', label: 'Email' },
    { key: 'status', label: 'Status', type: 'badge' },
    { key: 'verified', label: 'Verified', type: 'badge' },
    { key: 'productCount', label: 'Products', type: 'number' },
    { key: 'rating', label: 'Rating', type: 'number' },
    { key: 'createdAt', label: 'Joined', type: 'date' },
  ];

  actions: TableAction[] = [
    { label: 'View', icon: 'bi bi-eye', class: 'btn-outline-primary', handler: (row) => this.router.navigate(['/vendors', row.id]) },
    { label: 'Approve', icon: 'bi bi-check-circle', class: 'btn-outline-success', handler: (row) => this.approveVendor(row),
      visible: (row) => row.status === 'pending' },
    { label: 'Reject', icon: 'bi bi-x-circle', class: 'btn-outline-danger', handler: (row) => this.rejectVendor(row),
      visible: (row) => row.status === 'pending' },
  ];

  constructor(
    private api: ApiService,
    public router: Router,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.loadVendors();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadVendors(): void {
    this.loading = true;
    this.error = null;
    this.api.getPaginated<VendorProfile>('/api/v1/admin/vendors', this.page, this.pageSize)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.vendors = res.data.map(v => ({ ...v, verified: v.status === 'approved' }));
          this.total = res.total;
          this.loading = false;
        },
        error: () => {
          this.error = 'Failed to load vendors';
          this.loading = false;
        },
      });
  }

  approveVendor(vendor: VendorProfile): void {
    this.api.post(`/api/v1/admin/vendors/${vendor.id}/approve`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { this.toastr.success('Vendor approved'); this.loadVendors(); },
        error: () => this.toastr.error('Failed to approve vendor'),
      });
  }

  rejectVendor(vendor: VendorProfile): void {
    this.api.post(`/api/v1/admin/vendors/${vendor.id}/reject`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { this.toastr.success('Vendor rejected'); this.loadVendors(); },
        error: () => this.toastr.error('Failed to reject vendor'),
      });
  }

  onPageChange(page: number): void { this.page = page; this.loadVendors(); }
  onSortChange(sort: { key: string; dir: 'asc' | 'desc' }): void { this.loadVendors(); }
  onFilterChange(filters: Record<string, any>): void { this.page = 1; this.loadVendors(); }
  retry(): void { this.loadVendors(); }
}
