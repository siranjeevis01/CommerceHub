import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { VendorProductsPage } from './vendor-products.page';

@NgModule({
  imports: [RouterModule.forChild([{ path: '', component: VendorProductsPage }])],
  exports: [RouterModule],
})
export class VendorProductsRoutingModule {}
