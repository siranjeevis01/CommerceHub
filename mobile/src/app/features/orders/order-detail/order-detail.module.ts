import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { OrderDetailRoutingModule } from './order-detail-routing.module';
import { OrderDetailPage } from './order-detail.page';

@NgModule({
  imports: [
    CommonModule,
    IonicModule,
    OrderDetailRoutingModule,
  ],
  declarations: [OrderDetailPage],
})
export class OrderDetailModule {}
