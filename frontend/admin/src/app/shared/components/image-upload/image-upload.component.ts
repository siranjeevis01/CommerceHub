import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-image-upload',
  templateUrl: './image-upload.component.html',
  styleUrls: ['./image-upload.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ImageUploadComponent {
  @Input() label = 'Upload Image';
  @Input() multiple = false;
  @Input() accept = 'image/*';
  @Input() maxSize = 5;
  @Input() images: string[] = [];
  @Input() disabled = false;

  @Output() imagesChange = new EventEmitter<string[]>();
  @Output() imagesAdded = new EventEmitter<string[]>();

  dragOver = false;

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = false;
    const files = event.dataTransfer?.files;
    if (files) this.handleFiles(Array.from(files));
  }

  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.handleFiles(Array.from(input.files));
      input.value = '';
    }
  }

  private handleFiles(files: File[]): void {
    const validFiles = files.filter(f => f.size <= this.maxSize * 1024 * 1024);
    if (validFiles.length === 0) return;

    const urls: string[] = [];
    let loaded = 0;
    validFiles.forEach(file => {
      const reader = new FileReader();
      reader.onload = (e) => {
        urls.push(e.target?.result as string);
        loaded++;
        if (loaded === validFiles.length) {
          if (this.multiple) {
            this.images = [...this.images, ...urls];
          } else {
            this.images = urls;
          }
          this.imagesChange.emit(this.images);
          this.imagesAdded.emit(urls);
        }
      };
      reader.readAsDataURL(file);
    });
  }

  removeImage(index: number): void {
    this.images.splice(index, 1);
    this.imagesChange.emit([...this.images]);
  }

  addUrl(): void {
    const url = prompt('Enter image URL:');
    if (url) {
      this.images = [...this.images, url];
      this.imagesChange.emit(this.images);
    }
  }
}
