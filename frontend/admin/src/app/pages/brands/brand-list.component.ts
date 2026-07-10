import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ApiService } from '@shared/services/api.service';
import { Brand } from '@shared/models';
import { Subject, takeUntil } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-brand-list',
  templateUrl: './brand-list.component.html',
  styleUrls: ['./brand-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BrandListComponent implements OnInit, OnDestroy {
  brands: Brand[] = [];
  loading = false;
  error: string | null = null;
  showForm = false;
  editingBrand: Brand | null = null;
  formName = '';
  formSlug = '';
  formLogoUrl = '';
  formIsActive = true;

  private destroy$ = new Subject<void>();

  constructor(
    private api: ApiService,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.loadBrands();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadBrands(): void {
    this.loading = true;
    this.error = null;
    this.api.get<Brand[]>('/api/v1/brands')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.success) this.brands = res.data;
          this.loading = false;
        },
        error: () => {
          this.error = 'Failed to load brands';
          this.loading = false;
        },
      });
  }

  openCreateForm(): void {
    this.editingBrand = null;
    this.formName = '';
    this.formSlug = '';
    this.formLogoUrl = '';
    this.formIsActive = true;
    this.showForm = true;
  }

  openEditForm(brand: Brand): void {
    this.editingBrand = brand;
    this.formName = brand.name;
    this.formSlug = brand.slug;
    this.formLogoUrl = brand.logoUrl;
    this.formIsActive = brand.isActive;
    this.showForm = true;
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingBrand = null;
  }

  saveBrand(): void {
    if (!this.formName || !this.formSlug) {
      this.toastr.error('Name and slug are required');
      return;
    }
    const payload = { name: this.formName, slug: this.formSlug, logoUrl: this.formLogoUrl, isActive: this.formIsActive };
    const request = this.editingBrand
      ? this.api.put<Brand>(`/api/v1/admin/brands/${this.editingBrand.id}`, payload)
      : this.api.post<Brand>('/api/v1/admin/brands', payload);

    request.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastr.success(this.editingBrand ? 'Brand updated' : 'Brand created');
        this.cancelForm();
        this.loadBrands();
      },
      error: () => this.toastr.error('Failed to save brand'),
    });
  }

  toggleBrand(brand: Brand): void {
    this.api.put(`/api/v1/admin/brands/${brand.id}`, { isActive: !brand.isActive })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { brand.isActive = !brand.isActive; this.toastr.success('Brand updated'); },
        error: () => this.toastr.error('Failed to update brand'),
      });
  }

  deleteBrand(brand: Brand): void {
    if (!confirm(`Delete brand "${brand.name}"?`)) return;
    this.api.delete(`/api/v1/admin/brands/${brand.id}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { this.toastr.success('Brand deleted'); this.loadBrands(); },
        error: () => this.toastr.error('Failed to delete brand'),
      });
  }

  generateSlug(): void {
    if (!this.formSlug) {
      this.formSlug = this.formName.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');
    }
  }

  retry(): void { this.loadBrands(); }
}
