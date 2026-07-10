import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { ToastrModule } from 'ngx-toastr';

import { AdminRoutingModule } from './admin-routing.module';
import { AdminSharedModule } from './shared/shared.module';

import { AppComponent } from './app.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { NavbarComponent } from './components/navbar/navbar.component';

import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { UserListComponent } from './pages/users/user-list.component';
import { UserDetailComponent } from './pages/users/user-detail.component';
import { VendorListComponent } from './pages/vendors/vendor-list.component';
import { VendorDetailComponent } from './pages/vendors/vendor-detail.component';
import { ProductListComponent } from './pages/products/product-list.component';
import { ProductFormComponent } from './pages/products/product-form.component';
import { OrderListComponent } from './pages/orders/order-list.component';
import { OrderDetailComponent } from './pages/orders/order-detail.component';
import { CategoryListComponent } from './pages/categories/category-list.component';
import { BrandListComponent } from './pages/brands/brand-list.component';
import { CouponListComponent } from './pages/coupons/coupon-list.component';
import { CmsPagesComponent } from './pages/cms/cms-pages.component';
import { CmsEditorComponent } from './pages/cms/cms-editor.component';
import { AnalyticsDashboardComponent } from './pages/analytics/analytics-dashboard.component';
import { PayoutManagementComponent } from './pages/payouts/payout-management.component';

const pageComponents = [
  DashboardComponent,
  UserListComponent, UserDetailComponent,
  VendorListComponent, VendorDetailComponent,
  ProductListComponent, ProductFormComponent,
  OrderListComponent, OrderDetailComponent,
  CategoryListComponent,
  BrandListComponent,
  CouponListComponent,
  CmsPagesComponent, CmsEditorComponent,
  AnalyticsDashboardComponent,
  PayoutManagementComponent,
];

@NgModule({
  declarations: [
    AppComponent,
    SidebarComponent,
    NavbarComponent,
    ...pageComponents,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    HttpClientModule,
    AdminSharedModule,
    AdminRoutingModule,
    ToastrModule.forRoot({
      positionClass: 'toast-top-right',
      timeOut: 3000,
      closeButton: true,
      progressBar: true,
    }),
  ],
  exports: [AdminRoutingModule, AppComponent],
})
export class AdminModule {}
