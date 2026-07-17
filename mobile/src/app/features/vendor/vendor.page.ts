import { Component } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-vendor',
  template: `
    <ion-tabs>
      <ion-tab-bar slot="bottom">
        <ion-tab-button tab="dashboard">
          <ion-icon name="speedometer-outline"></ion-icon>
          <ion-label>Dashboard</ion-label>
        </ion-tab-button>
        <ion-tab-button tab="products">
          <ion-icon name="cube-outline"></ion-icon>
          <ion-label>Products</ion-label>
        </ion-tab-button>
        <ion-tab-button tab="orders">
          <ion-icon name="receipt-outline"></ion-icon>
          <ion-label>Orders</ion-label>
        </ion-tab-button>
      </ion-tab-bar>
    </ion-tabs>
  `,
})
export class VendorPage {}
