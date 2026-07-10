import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '@shared/services/api.service';
import { Subject, takeUntil } from 'rxjs';
import { CmsPage } from '../../admin.models';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-cms-pages',
  templateUrl: './cms-pages.component.html',
  styleUrls: ['./cms-pages.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CmsPagesComponent implements OnInit, OnDestroy {
  pages: CmsPage[] = [];
  loading = false;
  error: string | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private api: ApiService,
    private router: Router,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.loadPages();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadPages(): void {
    this.loading = true;
    this.error = null;
    this.api.get<CmsPage[]>('/api/v1/admin/cms/pages')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.success) this.pages = res.data;
          this.loading = false;
        },
        error: () => {
          this.error = 'Failed to load CMS pages';
          this.loading = false;
        },
      });
  }

  createPage(): void {
    this.api.post<CmsPage>('/api/v1/admin/cms/pages', {
      title: 'New Page',
      slug: 'new-page',
      content: '',
      metaTitle: '',
      metaDescription: '',
      isPublished: false,
    }).pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.router.navigate(['/cms', res.data.id]);
          }
        },
        error: () => this.toastr.error('Failed to create page'),
      });
  }

  deletePage(page: CmsPage): void {
    if (!confirm(`Delete "${page.title}"?`)) return;
    this.api.delete(`/api/v1/admin/cms/pages/${page.id}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { this.toastr.success('Page deleted'); this.loadPages(); },
        error: () => this.toastr.error('Failed to delete page'),
      });
  }

  togglePublish(page: CmsPage): void {
    this.api.put(`/api/v1/admin/cms/pages/${page.id}`, { isPublished: !page.isPublished })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { page.isPublished = !page.isPublished; this.toastr.success('Page updated'); },
        error: () => this.toastr.error('Failed to update page'),
      });
  }

  retry(): void { this.loadPages(); }
}
