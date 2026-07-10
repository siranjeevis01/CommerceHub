import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { LoadingSpinnerComponent } from './loading-spinner/loading-spinner.component';
import { StarRatingComponent } from './star-rating/star-rating.component';
import { PriceDisplayComponent } from './price-display/price-display.component';
import { ConfirmDialogComponent } from './confirm-dialog/confirm-dialog.component';
import { DataTableComponent } from './data-table/data-table.component';
import { ClickOutsideDirective } from './click-outside/click-outside.directive';

const components = [
  LoadingSpinnerComponent,
  StarRatingComponent,
  PriceDisplayComponent,
  ConfirmDialogComponent,
  DataTableComponent
];

const directives = [
  ClickOutsideDirective
];

@NgModule({
  declarations: [...components, ...directives],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  exports: [...components, ...directives]
})
export class SharedModule { }
