import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { Observable, Subject, catchError, debounceTime, distinctUntilChanged, map, of, shareReplay, switchMap, tap } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { Product, PaginatedResponse } from '@shared/models';
import { VendorService } from '../../services/vendor.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-product-list',
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductListComponent implements OnInit {
  Math = Math;
  products$: Observable<PaginatedResponse<Product> | null>;
  loading = true;
  page = 1;
  pageSize = 20;
  searchTerm = '';
  statusFilter = '';
  stockFilter = '';

  private searchSubject = new Subject<string>();
  selectedIds: Set<number> = new Set();

  constructor(
    private vendorService: VendorService,
    private toastr: ToastrService,
  ) {
    this.products$ = of(null);
  }

  ngOnInit(): void {
    this.loadProducts();

    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      tap(() => { this.page = 1; this.loadProducts(); }),
    ).subscribe();
  }

  loadProducts(): void {
    this.loading = true;
    let params = new HttpParams();
    if (this.searchTerm) params = params.set('search', this.searchTerm);
    if (this.statusFilter) params = params.set('status', this.statusFilter);
    if (this.stockFilter) params = params.set('stockLevel', this.stockFilter);

    this.products$ = this.vendorService.getProducts(this.page, this.pageSize, params).pipe(
      tap(() => { this.loading = false; this.selectedIds.clear(); }),
      catchError(() => {
        this.loading = false;
        this.toastr.error('Failed to load products');
        return of(null);
      }),
      shareReplay(1)
    );
  }

  onSearch(value: string): void {
    this.searchTerm = value;
    this.searchSubject.next(value);
  }

  onStatusFilter(value: string): void {
    this.statusFilter = value;
    this.page = 1;
    this.loadProducts();
  }

  onStockFilter(value: string): void {
    this.stockFilter = value;
    this.page = 1;
    this.loadProducts();
  }

  getPages(totalPages: number): number[] {
    const pages: number[] = [];
    const start = Math.max(1, (this.page || 1) - 2);
    const end = Math.min(totalPages, start + 4);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  }

  onPageChange(newPage: number): void {
    this.page = newPage;
    this.loadProducts();
  }

  toggleSelect(id: number): void {
    if (this.selectedIds.has(id)) this.selectedIds.delete(id);
    else this.selectedIds.add(id);
  }

  toggleAll(data: Product[]): void {
    if (this.selectedIds.size === data.length) {
      this.selectedIds.clear();
    } else {
      data.forEach(p => this.selectedIds.add(p.id));
    }
  }

  bulkAction(action: 'activate' | 'deactivate' | 'delete'): void {
    const ids = Array.from(this.selectedIds);
    if (ids.length === 0) { this.toastr.warning('No products selected'); return; }
    this.vendorService.bulkAction(ids, action).subscribe({
      next: () => {
        this.toastr.success(`${ids.length} products ${action}d successfully`);
        this.loadProducts();
      },
      error: () => this.toastr.error('Failed to perform bulk action'),
    });
  }

  deleteProduct(id: number): void {
    if (!confirm('Are you sure you want to delete this product?')) return;
    this.vendorService.deleteProduct(id).subscribe({
      next: () => { this.toastr.success('Product deleted'); this.loadProducts(); },
      error: () => this.toastr.error('Failed to delete product'),
    });
  }
}
