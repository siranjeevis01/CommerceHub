import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';

export interface ColumnConfig {
  key: string;
  label: string;
  sortable?: boolean;
  filterable?: boolean;
  template?: string;
  width?: string;
  align?: 'start' | 'center' | 'end';
}

export interface SortEvent {
  column: string;
  direction: 'asc' | 'desc';
}

export interface PageEvent {
  page: number;
  pageSize: number;
}

@Component({
  standalone: false,
  selector: 'app-data-table',
  templateUrl: './data-table.component.html',
  styleUrls: ['./data-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DataTableComponent<T = any> {
  @Input() columns: ColumnConfig[] = [];
  @Input() data: T[] = [];
  @Input() total = 0;
  @Input() page = 1;
  @Input() pageSize = 10;
  @Input() pageSizeOptions = [10, 25, 50, 100];
  @Input() loading = false;
  @Input() selectable = false;
  @Input() selectedKeys: Set<any> = new Set();
  @Input() keyField = 'id';
  @Input() emptyMessage = 'No records found';
  @Input() sortColumn = '';
  @Input() sortDirection: 'asc' | 'desc' = 'asc';
  @Input() serverSide = false;

  @Output() sortChange = new EventEmitter<SortEvent>();
  @Output() pageChange = new EventEmitter<PageEvent>();
  @Output() selectionChange = new EventEmitter<Set<any>>();
  @Output() rowClick = new EventEmitter<T>();
  @Output() filterChange = new EventEmitter<{ column: string; value: string }>();

  get totalPages(): number {
    return Math.ceil(this.total / this.pageSize);
  }

  get pages(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.page - 2);
    const end = Math.min(this.totalPages, this.page + 2);
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  get startRecord(): number {
    return (this.page - 1) * this.pageSize + 1;
  }

  get endRecord(): number {
    return Math.min(this.page * this.pageSize, this.total);
  }

  onSort(column: string): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    this.sortChange.emit({ column: this.sortColumn, direction: this.sortDirection });
  }

  onPage(p: number): void {
    if (p < 1 || p > this.totalPages || p === this.page) return;
    this.page = p;
    this.pageChange.emit({ page: this.page, pageSize: this.pageSize });
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.page = 1;
    this.pageChange.emit({ page: this.page, pageSize: this.pageSize });
  }

  toggleSelect(key: any): void {
    const newSelection = new Set(this.selectedKeys);
    if (newSelection.has(key)) {
      newSelection.delete(key);
    } else {
      newSelection.add(key);
    }
    this.selectionChange.emit(newSelection);
  }

  toggleSelectAll(): void {
    if (this.isAllSelected()) {
      this.selectionChange.emit(new Set());
    } else {
      this.selectionChange.emit(new Set(this.data.map(item => (item as any)[this.keyField])));
    }
  }

  isAllSelected(): boolean {
    return this.data.length > 0 && this.data.every(item => this.selectedKeys.has((item as any)[this.keyField]));
  }

  getCellValue(item: T, key: string): any {
    return (item as any)[key];
  }

  trackByFn(index: number, item: T): any {
    return (item as any)[this.keyField] || index;
  }
}
