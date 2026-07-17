import { Component } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-tabs',
  template: `
    <ion-tabs>
      <ion-tab-bar slot="bottom">
        <ion-tab-button tab="home">
          <ion-icon name="home"></ion-icon>
          <ion-label>Home</ion-label>
        </ion-tab-button>
        <ion-tab-button tab="search">
          <ion-icon name="search"></ion-icon>
          <ion-label>Search</ion-label>
        </ion-tab-button>
        <ion-tab-button tab="cart">
          <ion-icon name="cart"></ion-icon>
          <ion-label>Cart</ion-label>
        </ion-tab-button>
        <ion-tab-button tab="wishlist">
          <ion-icon name="heart"></ion-icon>
          <ion-label>Wishlist</ion-label>
        </ion-tab-button>
        <ion-tab-button tab="profile">
          <ion-icon name="person"></ion-icon>
          <ion-label>Profile</ion-label>
        </ion-tab-button>
      </ion-tab-bar>
    </ion-tabs>
  `
})
export class TabsComponent {}
