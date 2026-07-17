import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { FormsModule } from '@angular/forms';
import { VendorProductsRoutingModule } from './vendor-products-routing.module';
import { VendorProductsPage } from './vendor-products.page';

@NgModule({
  imports: [CommonModule, FormsModule, IonicModule, VendorProductsRoutingModule],
  declarations: [VendorProductsPage],
})
export class VendorProductsModule {}
