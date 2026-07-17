import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { ProductDetailRoutingModule } from './product-detail-routing.module';
import { ProductDetailPage } from './product-detail.page';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    ProductDetailRoutingModule,
  ],
  declarations: [ProductDetailPage],
})
export class ProductDetailModule {}
