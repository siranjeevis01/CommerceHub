import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '@shared/services/api.service';
import { Product, Category, VendorProfile } from '@shared/models';
import { Subject, takeUntil } from 'rxjs';
import { TableColumn, TableAction, FilterOption } from '../../admin.models';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-product-list',
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductListComponent implements OnInit, OnDestroy {
  products: Product[] = [];
  total = 0;
  page = 1;
  pageSize = 10;
  loading = false;
  error: string | null = null;
  selectedProducts: Product[] = [];

  private destroy$ = new Subject<void>();

  filters: FilterOption[] = [
    { key: 'search', label: 'Search', type: 'text', placeholder: 'Search products...' },
    { key: 'categoryId', label: 'Category', type: 'select', placeholder: 'All Categories', options: [] },
    { key: 'isActive', label: 'Status', type: 'select', placeholder: 'All', options: [
      { label: 'Active', value: true }, { label: 'Inactive', value: false },
    ]},
    { key: 'vendorId', label: 'Vendor', type: 'select', placeholder: 'All Vendors', options: [] },
  ];

  columns: TableColumn[] = [
    { key: 'name', label: 'Product Name', sortable: true },
    { key: 'price', label: 'Price', type: 'currency', sortable: true },
    { key: 'stockQuantity', label: 'Stock', type: 'number', sortable: true },
    { key: 'categoryName', label: 'Category' },
    { key: 'brandName', label: 'Brand' },
    { key: 'isActive', label: 'Status', type: 'badge' },
    { key: 'createdAt', label: 'Created', type: 'date', sortable: true },
  ];

  actions: TableAction[] = [
    { label: 'Edit', icon: 'bi bi-pencil', class: 'btn-outline-primary', handler: (row) => this.router.navigate(['/products', row.id, 'edit']) },
    { label: 'Toggle', icon: 'bi bi-toggle-on', class: 'btn-outline-warning', handler: (row) => this.toggleProduct(row) },
    { label: 'Delete', icon: 'bi bi-trash', class: 'btn-outline-danger', handler: (row) => this.deleteProduct(row) },
  ];

  constructor(
    private api: ApiService,
    public router: Router,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategoryOptions();
    this.loadVendorOptions();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProducts(): void {
    this.loading = true;
    this.error = null;
    this.api.getPaginated<Product>('/api/v1/admin/products', this.page, this.pageSize)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.products = res.data;
          this.total = res.total;
          this.loading = false;
        },
        error: () => {
          this.error = 'Failed to load products';
          this.loading = false;
        },
      });
  }

  private loadCategoryOptions(): void {
    this.api.get<Category[]>('/api/v1/categories')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.success) {
            const categoryFilter = this.filters.find(f => f.key === 'categoryId');
            if (categoryFilter) {
              categoryFilter.options = res.data.map(c => ({ label: c.name, value: c.id }));
            }
          }
        },
      });
  }

  private loadVendorOptions(): void {
    this.api.getPaginated<VendorProfile>('/api/v1/admin/vendors', 1, 1000)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          const vendorFilter = this.filters.find(f => f.key === 'vendorId');
          if (vendorFilter) {
            vendorFilter.options = res.data.map(v => ({ label: v.storeName, value: v.id }));
          }
        },
      });
  }

  toggleProduct(product: Product): void {
    const action = product.isActive ? 'deactivate' : 'activate';
    this.api.post(`/api/v1/admin/products/${product.id}/${action}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastr.success(`Product ${action}d`);
          this.loadProducts();
        },
        error: () => this.toastr.error(`Failed to ${action} product`),
      });
  }

  deleteProduct(product: Product): void {
    if (!confirm(`Delete "${product.name}"?`)) return;
    this.api.delete(`/api/v1/admin/products/${product.id}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { this.toastr.success('Product deleted'); this.loadProducts(); },
        error: () => this.toastr.error('Failed to delete product'),
      });
  }

  bulkToggle(active: boolean): void {
    if (this.selectedProducts.length === 0) return;
    const action = active ? 'activate' : 'deactivate';
    this.api.post(`/api/v1/admin/products/bulk/${action}`, { productIds: this.selectedProducts.map(p => p.id) })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastr.success(`${this.selectedProducts.length} products ${action}d`);
          this.loadProducts();
        },
        error: () => this.toastr.error(`Failed to bulk ${action}`),
      });
  }

  getStockLevel(product: Product): { class: string; label: string } {
    if (product.stockQuantity === 0) return { class: 'text-danger', label: 'Out of Stock' };
    if (product.stockQuantity < 10) return { class: 'text-warning', label: 'Low Stock' };
    return { class: 'text-success', label: 'In Stock' };
  }

  onPageChange(page: number): void { this.page = page; this.loadProducts(); }
  onSortChange(sort: { key: string; dir: 'asc' | 'desc' }): void { this.loadProducts(); }
  onFilterChange(filters: Record<string, any>): void { this.page = 1; this.loadProducts(); }
  onSelectionChange(selected: any[]): void { this.selectedProducts = selected; }
  retry(): void { this.loadProducts(); }
}
