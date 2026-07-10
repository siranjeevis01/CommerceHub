import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-rich-text-editor',
  templateUrl: './rich-text-editor.component.html',
  styleUrls: ['./rich-text-editor.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RichTextEditorComponent {
  @Input() label = '';
  @Input() placeholder = 'Write content here...';
  @Input() value = '';
  @Input() height = '300px';
  @Input() disabled = false;

  @Output() valueChange = new EventEmitter<string>();

  showHtml = false;

  execCmd(command: string, value?: string): void {
    document.execCommand(command, false, value);
    this.updateValue();
  }

  insertLink(): void {
    const url = prompt('Enter URL:');
    if (url) {
      this.execCmd('createLink', url);
    }
  }

  insertImage(): void {
    const url = prompt('Enter image URL:');
    if (url) {
      this.execCmd('insertImage', url);
    }
  }

  updateValue(): void {
    const editor = document.getElementById('rte-content');
    if (editor) {
      this.value = editor.innerHTML;
      this.valueChange.emit(this.value);
    }
  }

  onInput(): void {
    this.updateValue();
  }
}
