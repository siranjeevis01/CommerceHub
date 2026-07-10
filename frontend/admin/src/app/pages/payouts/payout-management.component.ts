import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ApiService } from '@shared/services/api.service';
import { Subject, takeUntil } from 'rxjs';
import { PayoutRequest } from '../../admin.models';
import { ToastrService } from 'ngx-toastr';
import { TableColumn, TableAction, FilterOption } from '../../admin.models';

@Component({
  standalone: false,
  selector: 'app-payout-management',
  templateUrl: './payout-management.component.html',
  styleUrls: ['./payout-management.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PayoutManagementComponent implements OnInit, OnDestroy {
  payouts: PayoutRequest[] = [];
  total = 0;
  page = 1;
  pageSize = 10;
  loading = false;
  error: string | null = null;
  selectedPayouts: PayoutRequest[] = [];
  batchProcessing = false;

  private destroy$ = new Subject<void>();

  filters: FilterOption[] = [
    { key: 'status', label: 'Status', type: 'select', placeholder: 'All Status', options: [
      { label: 'Pending', value: 'pending' }, { label: 'Approved', value: 'approved' },
      { label: 'Processing', value: 'processing' }, { label: 'Completed', value: 'completed' },
      { label: 'Rejected', value: 'rejected' },
    ]},
  ];

  columns: TableColumn[] = [
    { key: 'payoutNumber', label: 'Payout #' },
    { key: 'storeName', label: 'Store' },
    { key: 'amount', label: 'Amount', type: 'currency', sortable: true },
    { key: 'fee', label: 'Fee', type: 'currency' },
    { key: 'netAmount', label: 'Net Amount', type: 'currency', sortable: true },
    { key: 'status', label: 'Status', type: 'badge' },
    { key: 'requestedAt', label: 'Requested', type: 'date', sortable: true },
  ];

  actions: TableAction[] = [
    { label: 'Approve', icon: 'bi bi-check-circle', class: 'btn-outline-success', handler: (row) => this.updateStatus(row, 'approved'),
      visible: (row) => row.status === 'pending' },
    { label: 'Reject', icon: 'bi bi-x-circle', class: 'btn-outline-danger', handler: (row) => this.updateStatus(row, 'rejected'),
      visible: (row) => row.status === 'pending' },
    { label: 'Process', icon: 'bi bi-arrow-right', class: 'btn-outline-primary', handler: (row) => this.updateStatus(row, 'completed'),
      visible: (row) => row.status === 'approved' || row.status === 'processing' },
  ];

  constructor(
    private api: ApiService,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.loadPayouts();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get statusSummary(): { label: string; count: number; total: number }[] {
    const counts: Record<string, number> = { pending: 0, approved: 0, processing: 0, completed: 0, rejected: 0 };
    this.payouts.forEach(p => { counts[p.status] = (counts[p.status] || 0) + 1; });
    return [
      { label: 'Pending', count: counts['pending'], total: this.total },
      { label: 'Approved', count: counts['approved'], total: this.total },
      { label: 'Completed', count: counts['completed'], total: this.total },
      { label: 'Rejected', count: counts['rejected'], total: this.total },
    ];
  }

  loadPayouts(): void {
    this.loading = true;
    this.error = null;
    this.api.getPaginated<PayoutRequest>('/api/v1/admin/payouts', this.page, this.pageSize)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.payouts = res.data.map(p => ({ ...p, payoutNumber: p.payoutNumber || `PO-${p.id.toString().padStart(6, '0')}` }));
          this.total = res.total;
          this.loading = false;
        },
        error: () => {
          this.error = 'Failed to load payouts';
          this.loading = false;
        },
      });
  }

  updateStatus(payout: PayoutRequest, status: string): void {
    this.api.put(`/api/v1/admin/payouts/${payout.id}/status`, { status })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { this.toastr.success(`Payout ${status}`); this.loadPayouts(); },
        error: () => this.toastr.error('Failed to update payout'),
      });
  }

  batchProcess(): void {
    if (this.selectedPayouts.length === 0) return;
    this.batchProcessing = true;
    const ids = this.selectedPayouts.map(p => p.id);
    this.api.post('/api/v1/admin/payouts/batch/process', { payoutIds: ids })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastr.success(`${ids.length} payouts processed`);
          this.selectedPayouts = [];
          this.loadPayouts();
          this.batchProcessing = false;
        },
        error: () => { this.toastr.error('Batch processing failed'); this.batchProcessing = false; },
      });
  }

  get totalPendingAmount(): number {
    return this.payouts.filter(p => p.status === 'pending').reduce((sum, p) => sum + p.netAmount, 0);
  }

  onPageChange(page: number): void { this.page = page; this.loadPayouts(); }
  onSortChange(sort: { key: string; dir: 'asc' | 'desc' }): void { this.loadPayouts(); }
  onFilterChange(filters: Record<string, any>): void { this.page = 1; this.loadPayouts(); }
  onSelectionChange(selected: any[]): void { this.selectedPayouts = selected; }
  retry(): void { this.loadPayouts(); }
}
