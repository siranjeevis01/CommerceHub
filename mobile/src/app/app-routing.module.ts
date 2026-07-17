import { NgModule } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';
import { AuthGuard, GuestGuard, AdminGuard, VendorGuard } from './core/guards/auth.guard';

const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule),
    canActivate: [GuestGuard]
  },
  {
    path: 'tabs',
    loadChildren: () => import('./tabs/tabs.module').then(m => m.TabsModule)
  },
  {
    path: 'product/:id',
    loadChildren: () => import('./features/products/product-detail/product-detail.module').then(m => m.ProductDetailModule)
  },
  {
    path: 'category/:slug',
    loadChildren: () => import('./features/products/category-products/category-products.module').then(m => m.CategoryProductsModule)
  },
  {
    path: 'checkout',
    loadChildren: () => import('./features/checkout/checkout.module').then(m => m.CheckoutModule),
    canActivate: [AuthGuard]
  },
  {
    path: 'order/:id',
    loadChildren: () => import('./features/orders/order-detail/order-detail.module').then(m => m.OrderDetailModule),
    canActivate: [AuthGuard]
  },
  {
    path: 'vendor',
    loadChildren: () => import('./features/vendor/vendor.module').then(m => m.VendorModule),
    canActivate: [VendorGuard]
  },
  {
    path: 'admin',
    loadChildren: () => import('./features/admin/admin.module').then(m => m.AdminModule),
    canActivate: [AdminGuard]
  },
  {
    path: '',
    redirectTo: 'tabs/home',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: 'tabs/home'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { preloadingStrategy: PreloadAllModules })],
  exports: [RouterModule]
})
export class AppRoutingModule {}
