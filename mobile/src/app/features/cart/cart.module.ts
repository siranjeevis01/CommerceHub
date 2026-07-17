import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { CartRoutingModule } from './cart-routing.module';
import { CartPage } from './cart.page';

@NgModule({
  imports: [
    CommonModule,
    ReactiveFormsModule,
    IonicModule,
    CartRoutingModule,
  ],
  declarations: [CartPage],
})
export class CartModule {}
