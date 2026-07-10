import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@shared/services/api.service';
import { CmsPage } from '../../admin.models';
import { Subject, switchMap, takeUntil } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: false,
  selector: 'app-cms-editor',
  templateUrl: './cms-editor.component.html',
  styleUrls: ['./cms-editor.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CmsEditorComponent implements OnInit, OnDestroy {
  page: CmsPage | null = null;
  loading = true;
  saving = false;
  error: string | null = null;

  title = '';
  slug = '';
  content = '';
  metaTitle = '';
  metaDescription = '';
  isPublished = false;

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private api: ApiService,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap(params => this.api.get<CmsPage>(`/api/v1/admin/cms/pages/${params.get('id')}`)),
      takeUntil(this.destroy$),
    ).subscribe({
      next: (res) => {
        if (res.success) {
          this.page = res.data;
          this.title = res.data.title;
          this.slug = res.data.slug;
          this.content = res.data.content;
          this.metaTitle = res.data.metaTitle;
          this.metaDescription = res.data.metaDescription;
          this.isPublished = res.data.isPublished;
        }
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load page';
        this.loading = false;
      },
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  generateSlug(): void {
    if (!this.slug && this.title) {
      this.slug = this.title.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');
    }
  }

  save(): void {
    if (!this.title || !this.slug) {
      this.toastr.error('Title and slug are required');
      return;
    }
    this.saving = true;
    const payload = { title: this.title, slug: this.slug, content: this.content, metaTitle: this.metaTitle, metaDescription: this.metaDescription, isPublished: this.isPublished };

    this.api.put<CmsPage>(`/api/v1/admin/cms/pages/${this.page!.id}`, payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastr.success('Page saved');
          this.saving = false;
        },
        error: () => {
          this.toastr.error('Failed to save page');
          this.saving = false;
        },
      });
  }

  retry(): void {
    this.loading = true;
    this.error = null;
    this.ngOnInit();
  }
}
