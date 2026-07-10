import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DataTableComponent } from './components/data-table/data-table.component';
import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';
import { StatusBadgeComponent } from './components/status-badge/status-badge.component';
import { StatCardComponent } from './components/stat-card/stat-card.component';
import { ImageUploadComponent } from './components/image-upload/image-upload.component';
import { RichTextEditorComponent } from './components/rich-text-editor/rich-text-editor.component';

const components = [
  DataTableComponent,
  ConfirmDialogComponent,
  StatusBadgeComponent,
  StatCardComponent,
  ImageUploadComponent,
  RichTextEditorComponent,
];

@NgModule({
  declarations: [...components],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  exports: [...components],
})
export class AdminSharedModule {}
