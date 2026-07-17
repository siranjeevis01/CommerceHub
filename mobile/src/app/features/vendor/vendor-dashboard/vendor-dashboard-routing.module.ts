import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { VendorDashboardPage } from './vendor-dashboard.page';

@NgModule({
  imports: [RouterModule.forChild([{ path: '', component: VendorDashboardPage }])],
  exports: [RouterModule],
})
export class VendorDashboardRoutingModule {}
