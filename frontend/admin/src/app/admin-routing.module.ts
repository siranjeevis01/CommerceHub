import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '@shared/guards/auth.guard';
import { RoleGuard } from '@shared/guards/role.guard';
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

const routes: Routes = [
  {
    path: '',
    canActivateChild: [AuthGuard, RoleGuard],
    data: { roles: ['Admin'] },
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'users', component: UserListComponent },
      { path: 'users/:id', component: UserDetailComponent },
      { path: 'vendors', component: VendorListComponent },
      { path: 'vendors/:id', component: VendorDetailComponent },
      { path: 'products', component: ProductListComponent },
      { path: 'products/create', component: ProductFormComponent },
      { path: 'products/:id/edit', component: ProductFormComponent },
      { path: 'orders', component: OrderListComponent },
      { path: 'orders/:id', component: OrderDetailComponent },
      { path: 'categories', component: CategoryListComponent },
      { path: 'brands', component: BrandListComponent },
      { path: 'coupons', component: CouponListComponent },
      { path: 'cms', component: CmsPagesComponent },
      { path: 'cms/:id', component: CmsEditorComponent },
      { path: 'analytics', component: AnalyticsDashboardComponent },
      { path: 'payouts', component: PayoutManagementComponent },
      { path: 'settings', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AdminRoutingModule {}
