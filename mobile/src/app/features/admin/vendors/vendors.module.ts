import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { VendorsRoutingModule } from './vendors-routing.module';
import { VendorsPage } from './vendors.page';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    VendorsRoutingModule,
  ],
  declarations: [VendorsPage],
})
export class VendorsModule {}
