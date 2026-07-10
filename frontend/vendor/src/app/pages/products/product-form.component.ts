import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, catchError, map, of, shareReplay } from 'rxjs';
import { Category, Brand, Product } from '@shared/models';
import { VendorService } from '../../services/vendor.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-product-form',
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductFormComponent implements OnInit {
  productForm: FormGroup;
  isEdit = false;
  productId?: number;
  loading = false;
  saving = false;

  categories$: Observable<Category[]>;
  brands$: Observable<Brand[]>;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private vendorService: VendorService,
    private toastr: ToastrService,
  ) {
    this.productForm = this.createForm();
    this.categories$ = this.vendorService.getCategories().pipe(
      map(r => r.data || []),
      catchError(() => of([])),
      shareReplay(1)
    );
    this.brands$ = this.vendorService.getBrands().pipe(
      map(r => r.data || []),
      catchError(() => of([])),
      shareReplay(1)
    );
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.productId = +id;
      this.loadProduct(+id);
    } else {
      this.addVariant();
    }
  }

  private createForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', Validators.required],
      shortDescription: ['', [Validators.maxLength(500)]],
      categoryId: [null, Validators.required],
      brandId: [null, Validators.required],
      price: [null, [Validators.required, Validators.min(0.01)]],
      comparePrice: [null, Validators.min(0)],
      sku: ['', [Validators.required, Validators.maxLength(50)]],
      stockQuantity: [0, [Validators.required, Validators.min(0)]],
      imageUrl: [''],
      images: this.fb.array([]),
      isActive: [true],
      variants: this.fb.array([]),
      tags: [''],
    });
  }

  get variants(): FormArray {
    return this.productForm.get('variants') as FormArray;
  }

  get images(): FormArray {
    return this.productForm.get('images') as FormArray;
  }

  createVariant(): FormGroup {
    return this.fb.group({
      sku: ['', Validators.required],
      color: [''],
      size: [''],
      price: [null, [Validators.required, Validators.min(0.01)]],
      stockQuantity: [0, [Validators.required, Validators.min(0)]],
      imageUrl: [''],
    });
  }

  addVariant(): void {
    this.variants.push(this.createVariant());
  }

  removeVariant(index: number): void {
    this.variants.removeAt(index);
  }

  addImage(): void {
    this.images.push(this.fb.control(''));
  }

  removeImage(index: number): void {
    this.images.removeAt(index);
  }

  private loadProduct(id: number): void {
    this.loading = true;
    this.vendorService.getProduct(id).subscribe({
      next: (res) => {
        const p = res.data;
        this.productForm.patchValue({
          name: p.name,
          description: p.description,
          shortDescription: p.shortDescription,
          categoryId: p.categoryId,
          brandId: p.brandId,
          price: p.price,
          comparePrice: p.comparePrice,
          sku: p.sku,
          stockQuantity: p.stockQuantity,
          imageUrl: p.imageUrl,
          isActive: p.isActive,
          tags: p.tags?.join(', '),
        });
        this.images.clear();
        p.images?.forEach(img => this.images.push(this.fb.control(img)));
        this.variants.clear();
        p.variants?.forEach(v => {
          this.variants.push(this.fb.group({
            sku: [v.sku, Validators.required],
            color: [v.color || ''],
            size: [v.size || ''],
            price: [v.price, [Validators.required, Validators.min(0.01)]],
            stockQuantity: [v.stockQuantity, [Validators.required, Validators.min(0)]],
            imageUrl: [v.imageUrl || ''],
          }));
        });
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load product');
        this.router.navigate(['/products']);
      },
    });
  }

  onSubmit(draft: boolean = false): void {
    if (this.productForm.invalid) {
      Object.keys(this.productForm.controls).forEach(key => {
        const control = this.productForm.get(key);
        if (control?.invalid) control.markAsTouched();
      });
      this.toastr.warning('Please fill all required fields');
      return;
    }

    this.saving = true;
    const formValue = this.productForm.value;
    const data: any = { ...formValue };
    data.isActive = !draft && formValue.isActive;
    data.tags = formValue.tags ? formValue.tags.split(',').map((t: string) => t.trim()).filter(Boolean) : [];

    const request = this.isEdit
      ? this.vendorService.updateProduct(this.productId!, data)
      : this.vendorService.createProduct(data);

    request.subscribe({
      next: () => {
        this.saving = false;
        this.toastr.success(this.isEdit ? 'Product updated' : 'Product created');
        this.router.navigate(['/products']);
      },
      error: (err) => {
        this.saving = false;
        this.toastr.error(err.error?.message || 'Failed to save product');
      },
    });
  }
}
