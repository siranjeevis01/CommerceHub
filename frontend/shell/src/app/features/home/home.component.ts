import { Component, ChangeDetectionStrategy } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from '@shared/services/api.service';
import { ApiResponse, Category, Product } from '@shared/models';

@Component({
  standalone: false,
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeComponent {
  featuredProducts$: Observable<ApiResponse<Product[]>>;
  categories$: Observable<ApiResponse<Category[]>>;
  deals$: Observable<ApiResponse<Product[]>>;

  constructor(private api: ApiService) {
    const featuredParams = new HttpParams().set('isFeatured', 'true').set('pageSize', '8');
    this.featuredProducts$ = this.api.get<Product[]>('/api/v1/products', featuredParams);

    this.categories$ = this.api.get<Category[]>('/api/v1/categories');

    const dealParams = new HttpParams().set('hasDeal', 'true').set('pageSize', '4');
    this.deals$ = this.api.get<Product[]>('/api/v1/products', dealParams);
  }
}
