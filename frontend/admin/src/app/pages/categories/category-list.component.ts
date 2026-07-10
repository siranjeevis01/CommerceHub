import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ApiService } from '@shared/services/api.service';
import { Category } from '@shared/models';
import { Subject, takeUntil } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-category-list',
  templateUrl: './category-list.component.html',
  styleUrls: ['./category-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoryListComponent implements OnInit, OnDestroy {
  categories: Category[] = [];
  loading = false;
  error: string | null = null;
  editingCategory: Category | null = null;
  editName = '';
  editSlug = '';
  editDescription = '';
  editParentId: number | null = null;
  showForm = false;
  draggedItem: Category | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private api: ApiService,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCategories(): void {
    this.loading = true;
    this.error = null;
    this.api.get<Category[]>('/api/v1/categories')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.categories = res.data;
          }
          this.loading = false;
        },
        error: () => {
          this.error = 'Failed to load categories';
          this.loading = false;
        },
      });
  }

  openCreateForm(): void {
    this.editingCategory = null;
    this.editName = '';
    this.editSlug = '';
    this.editDescription = '';
    this.editParentId = null;
    this.showForm = true;
  }

  openEditForm(category: Category): void {
    this.editingCategory = category;
    this.editName = category.name;
    this.editSlug = category.slug;
    this.editDescription = category.description;
    this.editParentId = category.parentId || null;
    this.showForm = true;
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingCategory = null;
  }

  saveCategory(): void {
    if (!this.editName || !this.editSlug) {
      this.toastr.error('Name and slug are required');
      return;
    }

    const payload = {
      name: this.editName,
      slug: this.editSlug,
      description: this.editDescription,
      parentId: this.editParentId || null,
    };

    const request = this.editingCategory
      ? this.api.put<Category>(`/api/v1/admin/categories/${this.editingCategory.id}`, payload)
      : this.api.post<Category>('/api/v1/admin/categories', payload);

    request.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastr.success(this.editingCategory ? 'Category updated' : 'Category created');
        this.cancelForm();
        this.loadCategories();
      },
      error: () => this.toastr.error('Failed to save category'),
    });
  }

  deleteCategory(category: Category): void {
    if (!confirm(`Delete "${category.name}" and all subcategories?`)) return;
    this.api.delete(`/api/v1/admin/categories/${category.id}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { this.toastr.success('Category deleted'); this.loadCategories(); },
        error: () => this.toastr.error('Failed to delete category'),
      });
  }

  onDragStart(category: Category): void {
    this.draggedItem = category;
  }

  onDragOver(event: DragEvent, category: Category): void {
    event.preventDefault();
    if (this.draggedItem && this.draggedItem.id !== category.id) {
      (event.target as HTMLElement).closest('.category-row')?.classList.add('drag-over');
    }
  }

  onDragLeave(event: DragEvent): void {
    (event.target as HTMLElement).closest('.category-row')?.classList.remove('drag-over');
  }

  onDrop(event: DragEvent, target: Category): void {
    event.preventDefault();
    (event.target as HTMLElement).closest('.category-row')?.classList.remove('drag-over');
    if (!this.draggedItem || this.draggedItem.id === target.id) return;

    this.api.put(`/api/v1/admin/categories/${this.draggedItem.id}/reorder`, { targetId: target.id })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { this.toastr.success('Category reordered'); this.loadCategories(); },
        error: () => this.toastr.error('Failed to reorder'),
      });
    this.draggedItem = null;
  }

  generateSlug(): void {
    if (!this.editSlug) {
      this.editSlug = this.editName.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');
    }
  }

  retry(): void { this.loadCategories(); }
}
