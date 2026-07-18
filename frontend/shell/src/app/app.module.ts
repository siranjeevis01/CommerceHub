import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { ToastrModule } from 'ngx-toastr';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HeaderComponent } from './core/header/header.component';
import { FooterComponent } from './core/footer/footer.component';
import { NotificationDropdownComponent } from './core/notification-dropdown/notification-dropdown.component';
import { HomeComponent } from './features/home/home.component';
import { NotFoundComponent } from './features/not-found/not-found.component';
import { SharedModule } from './shared/shared.module';
import { JwtInterceptor, ErrorInterceptor } from '@shared/interceptors';

@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    FooterComponent,
    NotificationDropdownComponent,
    HomeComponent,
    NotFoundComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    AppRoutingModule,
    SharedModule,
    ToastrModule.forRoot({
      positionClass: 'toast-top-right',
      timeOut: 5000,
      closeButton: true,
      progressBar: false,
      preventDuplicates: true,
      newestOnTop: true
    })
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
