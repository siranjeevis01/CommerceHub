import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import('./app/app.module').then(m => platformBrowserDynamic().bootstrapModule(m.AppModule));
