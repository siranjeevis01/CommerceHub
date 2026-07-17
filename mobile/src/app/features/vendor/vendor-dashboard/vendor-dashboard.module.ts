import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { FormsModule } from '@angular/forms';
import { VendorDashboardRoutingModule } from './vendor-dashboard-routing.module';
import { VendorDashboardPage } from './vendor-dashboard.page';

@NgModule({
  imports: [CommonModule, FormsModule, IonicModule, VendorDashboardRoutingModule],
  declarations: [VendorDashboardPage],
})
export class VendorDashboardModule {}
