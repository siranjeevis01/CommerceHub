import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { VendorOrdersPage } from './vendor-orders.page';

@NgModule({
  imports: [RouterModule.forChild([{ path: '', component: VendorOrdersPage }])],
  exports: [RouterModule],
})
export class VendorOrdersRoutingModule {}
