import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { CategoryProductsRoutingModule } from './category-products-routing.module';
import { CategoryProductsPage } from './category-products.page';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    CategoryProductsRoutingModule,
  ],
  declarations: [CategoryProductsPage],
})
export class CategoryProductsModule {}
