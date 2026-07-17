import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { WishlistRoutingModule } from './wishlist-routing.module';
import { WishlistPage } from './wishlist.page';

@NgModule({
  imports: [
    CommonModule,
    IonicModule,
    WishlistRoutingModule,
  ],
  declarations: [WishlistPage],
})
export class WishlistModule {}
