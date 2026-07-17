import { Component } from '@angular/core';
import { Platform } from '@ionic/angular';
import { SignalRService } from './core/services/signalr.service';
import { ThemeService } from './core/services/theme.service';

@Component({
  standalone: false,
  selector: 'app-root',
  template: `
    <ion-app>
      <ion-router-outlet></ion-router-outlet>
    </ion-app>
  `
})
export class AppComponent {
  constructor(
    private platform: Platform,
    private signalr: SignalRService,
    private theme: ThemeService
  ) {
    this.initializeApp();
  }

  async initializeApp() {
    this.platform.ready().then(async () => {
      await this.signalr.start();
    });
  }
}
