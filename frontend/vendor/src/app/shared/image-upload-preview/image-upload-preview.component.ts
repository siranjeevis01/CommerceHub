import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-image-upload-preview',
  templateUrl: './image-upload-preview.component.html',
  styleUrls: ['./image-upload-preview.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ImageUploadPreviewComponent {
  @Input() imageUrl = '';
  @Input() placeholder = 'https://via.placeholder.com/150';
  @Input() width = 150;
  @Input() height = 150;
  @Input() editable = true;

  @Output() imageChange = new EventEmitter<string>();

  editMode = false;
  tempUrl = '';

  get displayUrl(): string {
    return this.imageUrl || this.placeholder;
  }

  toggleEdit(): void {
    this.editMode = !this.editMode;
    this.tempUrl = this.imageUrl;
  }

  saveUrl(): void {
    this.imageChange.emit(this.tempUrl);
    this.editMode = false;
  }

  cancelEdit(): void {
    this.editMode = false;
  }

  removeImage(): void {
    this.imageChange.emit('');
  }

  onImageError(event: Event): void {
    (event.target as HTMLImageElement).src = this.placeholder;
  }
}
