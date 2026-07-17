import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { VendorRoutingModule } from './vendor-routing.module';
import { VendorPage } from './vendor.page';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    VendorRoutingModule,
  ],
  declarations: [VendorPage],
})
export class VendorModule {}
