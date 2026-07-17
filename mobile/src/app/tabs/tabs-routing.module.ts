import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: 'home',
    loadChildren: () => import('../features/home/home.module').then(m => m.HomePageModule)
  },
  {
    path: 'search',
    loadChildren: () => import('../features/search/search.module').then(m => m.SearchPageModule)
  },
  {
    path: 'cart',
    loadChildren: () => import('../features/cart/cart.module').then(m => m.CartModule)
  },
  {
    path: 'wishlist',
    loadChildren: () => import('../features/wishlist/wishlist.module').then(m => m.WishlistModule)
  },
  {
    path: 'orders',
    loadChildren: () => import('../features/orders/orders.module').then(m => m.OrdersModule)
  },
  {
    path: 'profile',
    loadChildren: () => import('../features/profile/profile.module').then(m => m.ProfileModule)
  },
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full'
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TabsRoutingModule {}
