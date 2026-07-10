import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '@shared/guards/auth.guard';
import { RoleGuard } from '@shared/guards/role.guard';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { ProductListComponent } from './pages/products/product-list.component';
import { ProductFormComponent } from './pages/products/product-form.component';
import { ProductDetailComponent } from './pages/products/product-detail.component';
import { OrderListComponent } from './pages/orders/order-list.component';
import { OrderDetailComponent } from './pages/orders/order-detail.component';
import { PayoutHistoryComponent } from './pages/payouts/payout-history.component';
import { CommissionReportComponent } from './pages/commissions/commission-report.component';
import { ReviewListComponent } from './pages/reviews/review-list.component';
import { StoreSettingsComponent } from './pages/store/store-settings.component';
import { VendorAnalyticsComponent } from './pages/analytics/vendor-analytics.component';

const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard, RoleGuard], data: { roles: ['Vendor'] } },
  { path: 'products', component: ProductListComponent, canActivate: [AuthGuard, RoleGuard], data: { roles: ['Vendor'] } },
  { path: 'products/create', component: ProductFormComponent, canActivate: [AuthGuard, RoleGuard], data: { roles: ['Vendor'] } },
  { path: 'products/:id/edit', component: ProductFormComponent, canActivate: [AuthGuard, RoleGuard], data: { roles: ['Vendor'] } },
  { path: 'products/:id', component: ProductDetailComponent, canActivate: [AuthGuard, RoleGuard], data: { roles: ['Vendor'] } },
  { path: 'orders', component: OrderListComponent, canActivate: [AuthGuard, RoleGuard], data: { roles: ['Vendor'] } },
  { path: 'orders/:id', component: OrderDetailComponent, canActivate: [AuthGuard, RoleGuard], data: { roles: ['Vendor'] } },
  { path: 'payouts', component: PayoutHistoryComponent, canActivate: [AuthGuard, RoleGuard], data: { roles: ['Vendor'] } },
  { path: 'commissions', component: CommissionReportComponent, canActivate: [AuthGuard, RoleGuard], data: { roles: ['Vendor'] } },
  { path: 'reviews', component: ReviewListComponent, canActivate: [AuthGuard, RoleGuard], data: { roles: ['Vendor'] } },
  { path: 'store', component: StoreSettingsComponent, canActivate: [AuthGuard, RoleGuard], data: { roles: ['Vendor'] } },
  { path: 'analytics', component: VendorAnalyticsComponent, canActivate: [AuthGuard, RoleGuard], data: { roles: ['Vendor'] } },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class VendorRoutingModule {}
