import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@shared/services/api.service';
import { HttpParams } from '@angular/common/http';
import {
  Product, ProductVariant, Category, Brand,
  Order, Review, VendorProfile, VendorPayout,
  ApiResponse, PaginatedResponse
} from '@shared/models';

export interface SalesDataPoint {
  date: string;
  revenue: number;
  orders: number;
}

export interface DashboardStats {
  totalProducts: number;
  activeListings: number;
  totalOrders: number;
  monthlyRevenue: number;
  yearlyRevenue: number;
  averageRating: number;
  lowStockCount: number;
}

export interface CommissionEntry {
  id: number;
  orderNumber: string;
  productName: string;
  orderAmount: number;
  commissionRate: number;
  commissionAmount: number;
  status: string;
  createdAt: string;
}

export interface PayoutSummary {
  currentBalance: number;
  pendingAmount: number;
  availableForPayout: number;
}

export interface AnalyticsData {
  revenue: { month: string; revenue: number }[];
  orderVolume: { week: string; orders: number }[];
  topProducts: { name: string; revenue: number; units: number }[];
  conversionRate: number;
  totalVisitors: number;
  totalSales: number;
}

@Injectable({ providedIn: 'root' })
export class VendorService {
  private base = '/api/v1/vendor';

  constructor(private api: ApiService) {}

  getDashboardStats(): Observable<ApiResponse<DashboardStats>> {
    return this.api.get<DashboardStats>(`${this.base}/dashboard/stats`);
  }

  getSalesData(days: number = 30): Observable<ApiResponse<SalesDataPoint[]>> {
    return this.api.get<SalesDataPoint[]>(`${this.base}/dashboard/sales`, new HttpParams().set('days', days.toString()));
  }

  getProducts(page: number = 1, pageSize: number = 20, params?: HttpParams): Observable<PaginatedResponse<Product>> {
    return this.api.getPaginated<Product>(`${this.base}/products`, page, pageSize, params);
  }

  getProduct(id: number): Observable<ApiResponse<Product>> {
    return this.api.get<Product>(`${this.base}/products/${id}`);
  }

  createProduct(data: FormData | Partial<Product>): Observable<ApiResponse<Product>> {
    return this.api.post<Product>(`${this.base}/products`, data);
  }

  updateProduct(id: number, data: FormData | Partial<Product>): Observable<ApiResponse<Product>> {
    return this.api.put<Product>(`${this.base}/products/${id}`, data);
  }

  deleteProduct(id: number): Observable<ApiResponse<void>> {
    return this.api.delete<void>(`${this.base}/products/${id}`);
  }

  bulkAction(ids: number[], action: string): Observable<ApiResponse<void>> {
    return this.api.post<void>(`${this.base}/products/bulk`, { ids, action });
  }

  getCategories(): Observable<ApiResponse<Category[]>> {
    return this.api.get<Category[]>('/api/v1/categories');
  }

  getBrands(): Observable<ApiResponse<Brand[]>> {
    return this.api.get<Brand[]>('/api/v1/brands');
  }

  getOrders(page: number = 1, pageSize: number = 20, params?: HttpParams): Observable<PaginatedResponse<Order>> {
    return this.api.getPaginated<Order>(`${this.base}/orders`, page, pageSize, params);
  }

  getOrder(id: number): Observable<ApiResponse<Order>> {
    return this.api.get<Order>(`${this.base}/orders/${id}`);
  }

  updateOrderStatus(id: number, status: string, trackingNumber?: string): Observable<ApiResponse<Order>> {
    return this.api.put<Order>(`${this.base}/orders/${id}/status`, { status, trackingNumber });
  }

  getPayouts(page: number = 1, pageSize: number = 20): Observable<PaginatedResponse<VendorPayout>> {
    return this.api.getPaginated<VendorPayout>(`${this.base}/payouts`, page, pageSize);
  }

  getPayoutSummary(): Observable<ApiResponse<PayoutSummary>> {
    return this.api.get<PayoutSummary>(`${this.base}/payouts/summary`);
  }

  requestPayout(): Observable<ApiResponse<VendorPayout>> {
    return this.api.post<VendorPayout>(`${this.base}/payouts/request`);
  }

  getCommissions(page: number = 1, pageSize: number = 20, params?: HttpParams): Observable<PaginatedResponse<CommissionEntry>> {
    return this.api.getPaginated<CommissionEntry>(`${this.base}/commissions`, page, pageSize, params);
  }

  getCommissionSummary(): Observable<ApiResponse<{ total: number; thisMonth: number; pending: number }>> {
    return this.api.get<{ total: number; thisMonth: number; pending: number }>(`${this.base}/commissions/summary`);
  }

  getReviews(page: number = 1, pageSize: number = 20, params?: HttpParams): Observable<PaginatedResponse<Review>> {
    return this.api.getPaginated<Review>(`${this.base}/reviews`, page, pageSize, params);
  }

  replyToReview(id: number, reply: string): Observable<ApiResponse<Review>> {
    return this.api.put<Review>(`${this.base}/reviews/${id}/reply`, { reply });
  }

  getStoreProfile(): Observable<ApiResponse<VendorProfile>> {
    return this.api.get<VendorProfile>(`${this.base}/store`);
  }

  updateStoreProfile(data: Partial<VendorProfile>): Observable<ApiResponse<VendorProfile>> {
    return this.api.put<VendorProfile>(`${this.base}/store`, data);
  }

  getAnalytics(): Observable<ApiResponse<AnalyticsData>> {
    return this.api.get<AnalyticsData>(`${this.base}/analytics`);
  }

  getRecentOrders(limit: number = 5): Observable<ApiResponse<Order[]>> {
    return this.api.get<Order[]>(`${this.base}/dashboard/recent-orders`, new HttpParams().set('limit', limit.toString()));
  }

  getLowStockProducts(threshold: number = 10): Observable<ApiResponse<Product[]>> {
    return this.api.get<Product[]>(`${this.base}/dashboard/low-stock`, new HttpParams().set('threshold', threshold.toString()));
  }

  getTopProducts(limit: number = 5): Observable<ApiResponse<Product[]>> {
    return this.api.get<Product[]>(`${this.base}/dashboard/top-products`, new HttpParams().set('limit', limit.toString()));
  }

  getRecentReviews(limit: number = 5): Observable<ApiResponse<Review[]>> {
    return this.api.get<Review[]>(`${this.base}/dashboard/recent-reviews`, new HttpParams().set('limit', limit.toString()));
  }
}
