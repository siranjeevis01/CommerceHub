import { Component, OnInit } from '@angular/core';
import { ApiService } from '@core/services/api.service';
import { Product } from '@core/models';

@Component({
  standalone: false,
  selector: 'app-vendor-products',
  templateUrl: './vendor-products.page.html',
  styleUrls: ['./vendor-products.page.scss'],
})
export class VendorProductsPage implements OnInit {
  products: Product[] = [];
  isLoading = false;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.isLoading = true;
    this.api.get<Product[]>('/api/v1/vendor/products').subscribe({
      next: (res) => {
        this.isLoading = false;
        this.products = res?.data ?? [];
      },
      error: () => { this.isLoading = false; },
    });
  }

  toggleActive(product: Product): void {
    const action = product.isActive ? 'deactivate' : 'activate';
    this.api.put(`/api/v1/vendor/products/${product.id}/${action}`).subscribe({
      next: () => { product.isActive = !product.isActive; },
    });
  }
}
