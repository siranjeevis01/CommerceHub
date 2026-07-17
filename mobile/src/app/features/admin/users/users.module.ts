import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { UsersRoutingModule } from './users-routing.module';
import { UsersPage } from './users.page';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    UsersRoutingModule,
  ],
  declarations: [UsersPage],
})
export class UsersModule {}
