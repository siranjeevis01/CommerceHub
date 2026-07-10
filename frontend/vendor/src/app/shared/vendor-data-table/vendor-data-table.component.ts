import { ChangeDetectionStrategy, Component, ContentChild, EventEmitter, Input, Output, TemplateRef } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-vendor-data-table',
  templateUrl: './vendor-data-table.component.html',
  styleUrls: ['./vendor-data-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VendorDataTableComponent {
  Math = Math;
  @Input() columns: { key: string; label: string; sortable?: boolean; width?: string }[] = [];
  @Input() data: any[] = [];
  @Input() total = 0;
  @Input() page = 1;
  @Input() pageSize = 20;
  @Input() loading = false;
  @Input() selectable = false;
  @Input() emptyMessage = 'No data found';
  @Input() emptyIcon = 'inbox';

  @Output() pageChange = new EventEmitter<number>();
  @Output() sortChange = new EventEmitter<{ key: string; direction: 'asc' | 'desc' }>();
  @Output() selectionChange = new EventEmitter<any[]>();

  @ContentChild('actions') actionsTemplate?: TemplateRef<any>;
  @ContentChild('customCell') customCellTemplate?: TemplateRef<any>;

  selectedIds: Set<number> = new Set();
  sortKey = '';
  sortDir: 'asc' | 'desc' = 'asc';
  allSelected = false;

  get totalPages(): number {
    return Math.ceil(this.total / this.pageSize);
  }

  toggleSelect(id: number): void {
    if (this.selectedIds.has(id)) {
      this.selectedIds.delete(id);
    } else {
      this.selectedIds.add(id);
    }
    this.allSelected = this.selectedIds.size === this.data.length;
    this.selectionChange.emit(this.getSelectedItems());
  }

  toggleSelectAll(): void {
    this.allSelected = !this.allSelected;
    if (this.allSelected) {
      this.data.forEach(item => this.selectedIds.add(item.id));
    } else {
      this.selectedIds.clear();
    }
    this.selectionChange.emit(this.getSelectedItems());
  }

  isSelected(id: number): boolean {
    return this.selectedIds.has(id);
  }

  getSelectedItems(): any[] {
    return this.data.filter(item => this.selectedIds.has(item.id));
  }

  sort(key: string): void {
    if (this.sortKey === key) {
      this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortKey = key;
      this.sortDir = 'asc';
    }
    this.sortChange.emit({ key: this.sortKey, direction: this.sortDir });
  }

  onPageChange(newPage: number): void {
    if (newPage < 1 || newPage > this.totalPages) return;
    this.page = newPage;
    this.selectedIds.clear();
    this.allSelected = false;
    this.pageChange.emit(newPage);
  }

  onRowClick(event: Event, row: any): void {
    const target = event.target as HTMLElement | null;
    if (target?.tagName !== 'INPUT') {
      // row click handled
    }
  }

  getPages(): (number | string)[] {
    const pages: (number | string)[] = [];
    const tp = this.totalPages;
    if (tp <= 7) {
      for (let i = 1; i <= tp; i++) pages.push(i);
    } else {
      pages.push(1);
      if (this.page > 3) pages.push('...');
      for (let i = Math.max(2, this.page - 1); i <= Math.min(tp - 1, this.page + 1); i++) {
        pages.push(i);
      }
      if (this.page < tp - 2) pages.push('...');
      pages.push(tp);
    }
    return pages;
  }
}
