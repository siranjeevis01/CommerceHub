import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { ToastrModule } from 'ngx-toastr';
import { NgxPaginationModule } from 'ngx-pagination';

import { VendorRoutingModule } from './vendor-routing.module';

import { AppComponent } from './app.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';

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

import { VendorDataTableComponent } from './shared/vendor-data-table/vendor-data-table.component';
import { ConfirmDialogComponent } from './shared/confirm-dialog/confirm-dialog.component';
import { StatusBadgeComponent } from './shared/status-badge/status-badge.component';
import { StatCardComponent } from './shared/stat-card/stat-card.component';
import { ImageUploadPreviewComponent } from './shared/image-upload-preview/image-upload-preview.component';
import { EmptyStateComponent } from './shared/empty-state/empty-state.component';
import { LoadingOverlayComponent } from './shared/loading-overlay/loading-overlay.component';

@NgModule({
  declarations: [
    AppComponent,
    SidebarComponent,
    DashboardComponent,
    ProductListComponent,
    ProductFormComponent,
    ProductDetailComponent,
    OrderListComponent,
    OrderDetailComponent,
    PayoutHistoryComponent,
    CommissionReportComponent,
    ReviewListComponent,
    StoreSettingsComponent,
    VendorAnalyticsComponent,
    VendorDataTableComponent,
    ConfirmDialogComponent,
    StatusBadgeComponent,
    StatCardComponent,
    ImageUploadPreviewComponent,
    EmptyStateComponent,
    LoadingOverlayComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    HttpClientModule,
    VendorRoutingModule,
    NgxPaginationModule,
    ToastrModule.forRoot({
      positionClass: 'toast-top-right',
      preventDuplicates: true,
      timeOut: 3000,
    }),
  ],
  exports: [VendorRoutingModule, AppComponent],
})
export class VendorModule {}
