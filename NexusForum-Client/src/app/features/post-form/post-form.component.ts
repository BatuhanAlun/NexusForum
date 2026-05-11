import { Component, Input, OnChanges, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Category } from '../../core/models/category.models';
import { CategoryService } from '../../core/services/category.service';
import { PostService } from '../../core/services/post.service';
import { MarkdownPipe } from '../../shared/markdown/markdown.pipe';
import { apiError } from '../../core/utils/category.utils';

@Component({
  selector: 'app-post-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, MarkdownPipe],
  template: `
    <div class="form-page">
      <div class="form-card">
        <div class="form-card-header">
          <h1>{{ isEdit ? 'Edit Post' : 'New Post' }}</h1>
          <a routerLink="/" class="btn btn-ghost btn-sm">Cancel</a>
        </div>

        @if (error()) {
          <div class="alert alert-error">{{ error() }}</div>
        }

        @if (loadingPost()) {
          <div class="loading-wrap">Loading post</div>
        } @else {
          <form [formGroup]="form" (ngSubmit)="submit()" class="post-form">
            <div class="form-group">
              <label for="title">Title</label>
              <input id="title" type="text" formControlName="title"
                placeholder="What's your question or topic?" />
              @if (form.get('title')?.invalid && form.get('title')?.touched) {
                <span class="field-error">Title must be 3–200 characters</span>
              }
            </div>

            <div class="form-group">
              <label for="category">Category</label>
              <select id="category" formControlName="categoryId">
                <option value="" disabled>Select a category</option>
                @for (cat of categories(); track cat.id) {
                  <option [value]="cat.id">{{ cat.name }}</option>
                }
              </select>
              @if (form.get('categoryId')?.invalid && form.get('categoryId')?.touched) {
                <span class="field-error">Please select a category</span>
              }
            </div>

            <div class="form-group">
              <label for="content">Content</label>
              <textarea id="content" formControlName="content" rows="12"
                placeholder="Describe your question or share your thoughts…"></textarea>
              @if (form.get('content')?.invalid && form.get('content')?.touched) {
                <span class="field-error">Content must be at least 10 characters</span>
              }
            </div>

            @if (form.get('content')?.value) {
              <div class="form-group preview-group">
                <label>Preview</label>
                <div class="preview-body markdown-body" [innerHTML]="form.get('content')!.value | markdown"></div>
              </div>
            }

            @if (!isEdit) {
              <div class="form-group form-group-row">
                <label class="toggle-label">
                  <input type="checkbox" formControlName="isPrivate" />
                  <span>Private thread</span>
                </label>
                <span class="toggle-hint">Only invited members can read and reply.</span>
              </div>
            }

            <div class="form-actions">
              <button type="submit" class="btn btn-primary" [disabled]="submitting()">
                {{ submitting() ? (isEdit ? 'Saving…' : 'Publishing…') : (isEdit ? 'Save Changes' : 'Publish Post') }}
              </button>
            </div>
          </form>
        }
      </div>
    </div>
  `,
  styles: [`
    .form-page {
      max-width: 800px; margin: 0 auto; padding: 2rem 1.5rem;
    }
    .form-card {
      background: var(--bg-surface); border: 1px solid var(--border);
      border-radius: var(--radius-lg); padding: 1.75rem;
    }
    .form-card-header {
      display: flex; align-items: center; justify-content: space-between;
      margin-bottom: 1.5rem;
    }
    .form-card-header h1 { font-size: 1.25rem; font-weight: 700; }
    .post-form { display: flex; flex-direction: column; gap: 1.25rem; }
    .form-actions { display: flex; justify-content: flex-end; padding-top: 0.5rem; }
    .form-group-row { flex-direction: row; align-items: center; gap: 1rem; flex-wrap: wrap; }
    .toggle-label { display: flex; align-items: center; gap: 0.5rem; cursor: pointer; font-weight: 500; }
    .toggle-label input[type="checkbox"] { width: 1rem; height: 1rem; accent-color: var(--accent); }
    .toggle-hint { font-size: 0.8125rem; color: var(--fg-muted); }
    .preview-group { border-top: 1px solid var(--border-subtle); padding-top: 1rem; }
    .preview-body {
      background: var(--bg-elevated); border: 1px solid var(--border);
      border-radius: var(--radius); padding: 0.875rem 1rem; min-height: 60px;
      line-height: 1.7; font-size: 0.9375rem;
    }
  `],
})
export class PostFormComponent implements OnChanges {
  private fb = inject(FormBuilder);
  private postService = inject(PostService);
  private categoryService = inject(CategoryService);
  private router = inject(Router);

  @Input() id?: string;

  get isEdit(): boolean { return !!this.id; }

  categories = signal<Category[]>([]);
  loadingPost = signal(false);
  submitting = signal(false);
  error = signal<string | null>(null);

  form = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
    categoryId: ['', Validators.required],
    content: ['', [Validators.required, Validators.minLength(10)]],
    isPrivate: [false],
  });

  constructor() {
    this.categoryService.getAll().subscribe(cats => this.categories.set(cats));
  }

  ngOnChanges(): void {
    if (this.id) {
      this.loadingPost.set(true);
      this.postService.getById(Number(this.id)).subscribe({
        next: post => {
          this.form.patchValue({
            title: post.title,
            categoryId: String(post.categoryId),
            content: post.content,
          });
          this.loadingPost.set(false);
        },
        error: () => {
          this.error.set('Post not found.');
          this.loadingPost.set(false);
        },
      });
    }
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.submitting.set(true);
    this.error.set(null);
    const { title, categoryId, content, isPrivate } = this.form.value;
    const payload = { title: title!, categoryId: Number(categoryId!), content: content!, isPrivate: isPrivate ?? false };

    const req$ = this.isEdit
      ? this.postService.update(Number(this.id!), payload)
      : this.postService.create(payload);

    req$.subscribe({
      next: post => this.router.navigate(['/posts', post.id]),
      error: (err) => { this.error.set(apiError(err)); this.submitting.set(false); },
    });
  }
}
