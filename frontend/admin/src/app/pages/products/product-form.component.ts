import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@shared/services/api.service';
import { Product, Category, Brand } from '@shared/models';
import { Subject, takeUntil } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-product-form',
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductFormComponent implements OnInit, OnDestroy {
  form: FormGroup;
  isEdit = false;
  productId: number | null = null;
  loading = false;
  saving = false;
  error: string | null = null;
  categories: Category[] = [];
  brands: Brand[] = [];

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private api: ApiService,
    private toastr: ToastrService,
  ) {
    this.form = this.createForm();
  }

  ngOnInit(): void {
    this.loadCategories();
    this.loadBrands();

    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.isEdit = true;
        this.productId = Number(id);
        this.loadProduct(this.productId);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createForm(): FormGroup {
    return this.fb.group({
      name: ['', Validators.required],
      slug: ['', Validators.required],
      description: [''],
      shortDescription: [''],
      categoryId: [null, Validators.required],
      brandId: [null, Validators.required],
      price: [null, [Validators.required, Validators.min(0)]],
      comparePrice: [null],
      sku: ['', Validators.required],
      stockQuantity: [0, [Validators.min(0)]],
      isActive: [true],
      tags: [''],
      metaTitle: [''],
      metaDescription: [''],
      mainImageUrl: [''],
      galleryImages: this.fb.array([]),
      variants: this.fb.array([]),
    });
  }

  get galleryImagesArray(): FormArray {
    return this.form.get('galleryImages') as FormArray;
  }

  get galleryImageControls(): FormControl[] {
    return this.galleryImagesArray.controls as FormControl[];
  }

  get variantsArray(): FormArray {
    return this.form.get('variants') as FormArray;
  }

  addGalleryImage(url: string = ''): void {
    this.galleryImagesArray.push(this.fb.control(url));
  }

  removeGalleryImage(index: number): void {
    this.galleryImagesArray.removeAt(index);
  }

  addVariant(): void {
    this.variantsArray.push(this.fb.group({
      sku: ['', Validators.required],
      color: [''],
      size: [''],
      price: [null, [Validators.required, Validators.min(0)]],
      stockQuantity: [0, [Validators.min(0)]],
      imageUrl: [''],
    }));
  }

  removeVariant(index: number): void {
    this.variantsArray.removeAt(index);
  }

  private loadProduct(id: number): void {
    this.loading = true;
    this.api.get<Product>(`/api/v1/admin/products/${id}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.patchForm(res.data);
          }
          this.loading = false;
        },
        error: () => {
          this.error = 'Failed to load product';
          this.loading = false;
        },
      });
  }

  private patchForm(product: Product): void {
    this.form.patchValue({
      name: product.name,
      slug: product.slug,
      description: product.description,
      shortDescription: product.shortDescription,
      categoryId: product.categoryId,
      brandId: product.brandId,
      price: product.price,
      comparePrice: product.comparePrice,
      sku: product.sku,
      stockQuantity: product.stockQuantity,
      isActive: product.isActive,
      tags: product.tags?.join(', ') || '',
      mainImageUrl: product.mainImageUrl || product.imageUrl || '',
    });

    const gallery = product.galleryImages || product.images || [];
    gallery.forEach(url => this.addGalleryImage(url));

    product.variants?.forEach(v => {
      this.variantsArray.push(this.fb.group({
        sku: [v.sku, Validators.required],
        color: [v.color || ''],
        size: [v.size || ''],
        price: [v.price, [Validators.required, Validators.min(0)]],
        stockQuantity: [v.stockQuantity, [Validators.min(0)]],
        imageUrl: [v.imageUrl || ''],
      }));
    });
  }

  private loadCategories(): void {
    this.api.get<Category[]>('/api/v1/categories')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => { if (res.success) this.categories = res.data; },
      });
  }

  private loadBrands(): void {
    this.api.get<Brand[]>('/api/v1/brands')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => { if (res.success) this.brands = res.data; },
      });
  }

  generateSlug(): void {
    const name = this.form.get('name')?.value;
    if (name && !this.form.get('slug')?.dirty) {
      this.form.patchValue({
        slug: name.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, ''),
      });
    }
  }

  onMainImageInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.form.patchValue({ mainImageUrl: value });
  }

  onImageError(event: Event): void {
    (event.target as HTMLImageElement).src = 'https://via.placeholder.com/150';
  }

  onSubmit(): void {
    if (this.form.invalid) {
      Object.keys(this.form.controls).forEach(key => {
        this.form.get(key)?.markAsTouched();
      });
      this.toastr.error('Please fix form errors');
      return;
    }

    this.saving = true;
    const formData = this.form.value;
    const payload = {
      ...formData,
      tags: formData.tags ? formData.tags.split(',').map((t: string) => t.trim()).filter(Boolean) : [],
      imageUrl: formData.mainImageUrl || '',
      images: formData.galleryImages || [],
    };

    const request = this.isEdit
      ? this.api.put<Product>(`/api/v1/admin/products/${this.productId}`, payload)
      : this.api.post<Product>('/api/v1/admin/products', payload);

    request.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastr.success(this.isEdit ? 'Product updated' : 'Product created');
        this.router.navigate(['/products']);
      },
      error: () => {
        this.toastr.error('Failed to save product');
        this.saving = false;
      },
    });
  }
}
