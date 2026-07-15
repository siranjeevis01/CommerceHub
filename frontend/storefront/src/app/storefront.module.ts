import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { StorefrontRoutingModule } from './storefront-routing.module';

import { AppComponent } from './app.component';

import { HomeComponent } from './pages/home/home.component';
import { ProductListComponent } from './pages/products/product-list.component';
import { ProductDetailComponent } from './pages/products/product-detail.component';
import { CartComponent } from './pages/cart/cart.component';
import { CheckoutComponent } from './pages/checkout/checkout.component';
import { SearchComponent } from './pages/search/search.component';
import { WishlistComponent } from './pages/wishlist/wishlist.component';
import { OrderHistoryComponent } from './pages/orders/order-history.component';
import { OrderDetailComponent } from './pages/orders/order-detail.component';
import { VendorStoreComponent } from './pages/vendor/vendor-store.component';

import { ProductCardComponent } from './shared/components/product-card/product-card.component';
import { ProductRatingComponent } from './shared/components/product-rating/product-rating.component';
import { PriceDisplayComponent } from './shared/components/price-display/price-display.component';
import { QuantitySelectorComponent } from './shared/components/quantity-selector/quantity-selector.component';
import { BreadcrumbComponent } from './shared/components/breadcrumb/breadcrumb.component';
import { PaginationFooterComponent } from './shared/components/pagination-footer/pagination-footer.component';
import { LoadingOverlayComponent } from './shared/components/loading-overlay/loading-overlay.component';
import { EmptyStateComponent } from './shared/components/empty-state/empty-state.component';
import { AiChatComponent } from './shared/components/ai-chat/ai-chat.component';

@NgModule({
  declarations: [
    AppComponent,
    AiChatComponent,
    HomeComponent,
    ProductListComponent,
    ProductDetailComponent,
    CartComponent,
    CheckoutComponent,
    SearchComponent,
    WishlistComponent,
    OrderHistoryComponent,
    OrderDetailComponent,
    VendorStoreComponent,
    ProductCardComponent,
    ProductRatingComponent,
    PriceDisplayComponent,
    QuantitySelectorComponent,
    BreadcrumbComponent,
    PaginationFooterComponent,
    LoadingOverlayComponent,
    EmptyStateComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    StorefrontRoutingModule,
  ],
  exports: [StorefrontRoutingModule],
})
export class StorefrontModule {}
