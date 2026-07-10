import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '@shared/guards/auth.guard';
import { ProfileComponent } from './profile.component';
import { AddressListComponent } from './address-list/address-list.component';
import { OrderHistoryComponent } from './order-history/order-history.component';
import { WishlistComponent } from './wishlist/wishlist.component';

const routes: Routes = [
  {
    path: '',
    canActivate: [AuthGuard],
    children: [
      { path: '', component: ProfileComponent },
      { path: 'addresses', component: AddressListComponent },
      { path: 'orders', component: OrderHistoryComponent },
      { path: 'orders/:id', component: OrderHistoryComponent },
      { path: 'wishlist', component: WishlistComponent }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ProfileRoutingModule { }
