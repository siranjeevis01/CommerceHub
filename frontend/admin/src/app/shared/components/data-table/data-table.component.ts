import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, TemplateRef } from '@angular/core';
import { TableColumn, TableAction, FilterOption } from '../../../admin.models';

@Component({
  standalone: false,
  selector: 'app-data-table',
  templateUrl: './data-table.component.html',
  styleUrls: ['./data-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataTableComponent {
  Math = Math;
  Object = Object;
  @Input() columns: TableColumn[] = [];
  @Input() data: any[] = [];
  @Input() total = 0;
  @Input() page = 1;
  @Input() pageSize = 10;
  @Input() loading = false;
  @Input() error: string | null = null;
  @Input() actions: TableAction[] = [];
  @Input() filters: FilterOption[] = [];
  @Input() selectable = false;
  @Input() emptyMessage = 'No data found';
  @Input() rowClickable = false;
  @Input() sortKey = '';
  @Input() sortDir: 'asc' | 'desc' = 'asc';
  @Input() actionTemplate: TemplateRef<any> | null = null;

  @Output() pageChange = new EventEmitter<number>();
  @Output() sortChange = new EventEmitter<{ key: string; dir: 'asc' | 'desc' }>();
  @Output() filterChange = new EventEmitter<Record<string, any>>();
  @Output() selectionChange = new EventEmitter<any[]>();
  @Output() rowClick = new EventEmitter<any>();

  selectedItems = new Set<any>();
  currentFilters: Record<string, any> = {};

  get totalPages(): number {
    return Math.ceil(this.total / this.pageSize);
  }

  get displayPages(): number[] {
    const total = this.totalPages;
    const current = this.page;
    const pages: number[] = [];
    const start = Math.max(1, current - 2);
    const end = Math.min(total, current + 2);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  }

  toggleSort(key: string): void {
    if (!this.columns.find(c => c.key === key)?.sortable) return;
    const dir = this.sortKey === key && this.sortDir === 'asc' ? 'desc' : 'asc';
    this.sortChange.emit({ key, dir });
  }

  onFilterChange(): void {
    this.filterChange.emit(this.currentFilters);
  }

  clearFilter(key: string): void {
    delete this.currentFilters[key];
    this.onFilterChange();
  }

  clearAllFilters(): void {
    this.currentFilters = {};
    this.onFilterChange();
  }

  toggleSelectAll(): void {
    if (this.selectedItems.size === this.data.length) {
      this.selectedItems.clear();
    } else {
      this.data.forEach(item => this.selectedItems.add(item));
    }
    this.selectionChange.emit(Array.from(this.selectedItems));
  }

  toggleSelect(item: any): void {
    if (this.selectedItems.has(item)) {
      this.selectedItems.delete(item);
    } else {
      this.selectedItems.add(item);
    }
    this.selectionChange.emit(Array.from(this.selectedItems));
  }

  isSelected(item: any): boolean {
    return this.selectedItems.has(item);
  }

  isAllSelected(): boolean {
    return this.data.length > 0 && this.selectedItems.size === this.data.length;
  }

  onRowClick(row: any): void {
    if (this.rowClickable) {
      this.rowClick.emit(row);
    }
  }

  pageSizeOptions = [10, 25, 50, 100];
  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.page = 1;
    this.pageChange.emit(this.page);
  }

  formatValue(row: any, col: TableColumn): string {
    const value = row[col.key];
    if (col.format) return col.format(value);
    if (value == null) return '-';
    if (col.type === 'currency') return `$${Number(value).toFixed(2)}`;
    if (col.type === 'date') return new Date(value).toLocaleDateString();
    if (col.type === 'number') return Number(value).toLocaleString();
    return String(value);
  }
}
