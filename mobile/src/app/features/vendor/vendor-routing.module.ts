import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { VendorPage } from './vendor.page';

const routes: Routes = [
  {
    path: '',
    component: VendorPage,
    children: [
      {
        path: 'dashboard',
        loadChildren: () => import('./vendor-dashboard/vendor-dashboard.module').then(m => m.VendorDashboardModule),
      },
      {
        path: 'products',
        loadChildren: () => import('./vendor-products/vendor-products.module').then(m => m.VendorProductsModule),
      },
      {
        path: 'orders',
        loadChildren: () => import('./vendor-orders/vendor-orders.module').then(m => m.VendorOrdersModule),
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class VendorRoutingModule {}
