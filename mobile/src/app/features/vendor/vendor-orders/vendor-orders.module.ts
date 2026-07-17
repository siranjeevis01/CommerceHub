import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { FormsModule } from '@angular/forms';
import { VendorOrdersRoutingModule } from './vendor-orders-routing.module';
import { VendorOrdersPage } from './vendor-orders.page';

@NgModule({
  imports: [CommonModule, FormsModule, IonicModule, VendorOrdersRoutingModule],
  declarations: [VendorOrdersPage],
})
export class VendorOrdersModule {}
