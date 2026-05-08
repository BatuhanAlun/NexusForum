import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { map } from 'rxjs';
import { Category } from '../../core/models/category.models';
import { PagedResult, PostSummaryDto } from '../../core/models/post.models';
import { AuthService } from '../../core/services/auth.service';
import { CategoryService } from '../../core/services/category.service';
import { PostService } from '../../core/services/post.service';
import { categoryColor, timeAgo } from '../../core/utils/category.utils';

// Re-export utils for template use
const _timeAgo = timeAgo;
const _categoryColor = categoryColor;

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="page">
      <div class="sidebar">
        <div class="sidebar-section">
          <h3 class="sidebar-title">Categories</h3>
          <nav class="cat-nav">
            <button class="cat-btn" [class.active]="!selectedCategoryId()" (click)="selectCategory()">
              All Posts
            </button>
            @for (cat of categories(); track cat.id) {
              <button class="cat-btn" [class.active]="selectedCategoryId() === cat.id" (click)="selectCategory(cat.id)">
                <span class="badge badge-{{ color(cat.name) }}">{{ cat.name }}</span>
              </button>
            }
          </nav>
        </div>

        <div class="sidebar-section about-box">
          <h3 class="sidebar-title">NexusForum</h3>
          <p class="about-text">A community for developers to ask questions, share knowledge, and level up together.</p>
          @if (auth.isLoggedIn()) {
            <a routerLink="/posts/new" class="btn btn-primary btn-full" style="margin-top:.75rem">+ New Post</a>
          } @else {
            <a routerLink="/register" class="btn btn-primary btn-full" style="margin-top:.75rem">Join the Community</a>
          }
        </div>
      </div>

      <main class="feed">
        <div class="feed-header">
          <div class="feed-title-row">
            <h2 class="feed-title">{{ selectedCategory()?.name ?? 'All Posts' }}</h2>
            @if (result()) {
              <span class="feed-count">{{ result()!.totalCount }} posts</span>
            }
          </div>
        </div>

        @if (loading()) {
          <div class="loading-wrap">Loading posts</div>
        } @else {
          @if (!result() || result()!.items.length === 0) {
            <div class="empty-state">
              <h3>No posts yet</h3>
              <p>Be the first to start a discussion in this category!</p>
            </div>
          } @else {
            <div class="post-list">
              @for (post of result()!.items; track post.id) {
                <article class="post-card" [routerLink]="['/posts', post.id]">
                  <div class="post-top">
                    <span class="badge badge-{{ color(post.categoryName) }}">{{ post.categoryName }}</span>
                    @if (post.isPrivate) {
                      <span class="lock-badge">🔒</span>
                    }
                    <span class="post-time">{{ ago(post.createdAt) }}</span>
                  </div>
                  <h3 class="post-title">{{ post.title }}</h3>
                  <div class="post-bottom">
                    <a class="post-author" [routerLink]="['/users', post.authorUsername]" (click)="$event.stopPropagation()">
                      <span class="avatar-xs">{{ post.authorUsername[0].toUpperCase() }}</span>
                      {{ post.authorUsername }}
                    </a>
                    <span class="post-comments">
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
                      </svg>
                      {{ post.commentCount }}
                    </span>
                  </div>
                </article>
              }
            </div>

            @if (result()!.totalPages > 1) {
              <div class="pagination">
                <button class="btn btn-secondary btn-sm"
                  [disabled]="!result()!.hasPreviousPage"
                  (click)="goToPage(currentPage() - 1)">
                  ← Prev
                </button>
                <span class="page-info">{{ currentPage() }} / {{ result()!.totalPages }}</span>
                <button class="btn btn-secondary btn-sm"
                  [disabled]="!result()!.hasNextPage"
                  (click)="goToPage(currentPage() + 1)">
                  Next →
                </button>
              </div>
            }
          }
        }
      </main>
    </div>
  `,
  styles: [`
    .page {
      max-width: 1200px; margin: 0 auto; padding: 2rem 1.5rem;
      display: grid; grid-template-columns: 240px 1fr; gap: 1.5rem;
    }
    .sidebar { display: flex; flex-direction: column; gap: 1rem; }
    .sidebar-section {
      background: var(--bg-surface); border: 1px solid var(--border);
      border-radius: var(--radius-lg); padding: 1rem;
    }
    .sidebar-title {
      font-size: 0.75rem; font-weight: 600; text-transform: uppercase;
      letter-spacing: 0.06em; color: var(--fg-subtle); margin-bottom: 0.75rem;
    }
    .cat-nav { display: flex; flex-direction: column; gap: 0.125rem; }
    .cat-btn {
      display: flex; align-items: center; gap: 0.5rem;
      padding: 0.4rem 0.625rem; border-radius: var(--radius);
      background: transparent; border: none; cursor: pointer;
      font-size: 0.875rem; color: var(--fg-muted);
      text-align: left; transition: all 0.15s; width: 100%;
    }
    .cat-btn:hover, .cat-btn.active {
      background: var(--bg-elevated); color: var(--fg-default);
    }
    .about-text { font-size: 0.8125rem; color: var(--fg-muted); line-height: 1.5; }

    .feed-header { margin-bottom: 1rem; }
    .feed-title-row { display: flex; align-items: baseline; gap: 0.75rem; }
    .feed-title { font-size: 1.125rem; font-weight: 700; }
    .feed-count { font-size: 0.8125rem; color: var(--fg-muted); }

    .post-list { display: flex; flex-direction: column; gap: 0.5rem; }
    .post-card {
      display: block;
      background: var(--bg-surface); border: 1px solid var(--border);
      border-radius: var(--radius-lg); padding: 1rem 1.25rem;
      cursor: pointer; transition: border-color 0.15s, background 0.15s;
      text-decoration: none !important; color: inherit;
    }
    .post-card:hover {
      border-color: var(--accent); background: var(--bg-elevated);
    }
    .post-top { display: flex; align-items: center; gap: 0.5rem; margin-bottom: 0.5rem; }
    .lock-badge { font-size: 0.8rem; opacity: 0.85; }
    .post-time { font-size: 0.75rem; color: var(--fg-subtle); margin-left: auto; }
    .post-title { font-size: 1rem; font-weight: 600; color: var(--fg-default); line-height: 1.4; margin-bottom: 0.75rem; }
    .post-bottom { display: flex; align-items: center; gap: 1rem; }
    .post-author {
      display: flex; align-items: center; gap: 0.375rem;
      font-size: 0.8125rem; color: var(--fg-muted);
      text-decoration: none !important; transition: color 0.15s;
    }
    .post-author:hover { color: var(--accent); }
    .post-comments {
      display: flex; align-items: center; gap: 0.25rem;
      font-size: 0.8125rem; color: var(--fg-subtle); margin-left: auto;
    }

    .pagination {
      display: flex; align-items: center; justify-content: center;
      gap: 1rem; margin-top: 1.5rem;
    }
    .page-info { font-size: 0.875rem; color: var(--fg-muted); }

    @media (max-width: 768px) {
      .page { grid-template-columns: 1fr; }
      .sidebar { display: none; }
    }
  `],
})
export class HomeComponent implements OnInit {
  private postService = inject(PostService);
  private categoryService = inject(CategoryService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  protected auth = inject(AuthService);
  protected color = _categoryColor;
  protected ago = _timeAgo;

  categories = signal<Category[]>([]);
  result = signal<PagedResult<PostSummaryDto> | null>(null);
  loading = signal(true);
  selectedCategoryId = signal<number | undefined>(undefined);
  currentPage = signal(1);

  selectedCategory = computed(() =>
    this.categories().find(c => c.id === this.selectedCategoryId())
  );

  ngOnInit(): void {
    this.categoryService.getAll().subscribe(cats => this.categories.set(cats));

    this.route.queryParamMap
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        map(params => ({
          categoryId: params.get('categoryId') ? Number(params.get('categoryId')) : undefined,
          page: params.get('page') ? Number(params.get('page')) : 1,
        }))
      )
      .subscribe(({ categoryId, page }) => {
        this.selectedCategoryId.set(categoryId);
        this.currentPage.set(page);
        this.loadPosts();
      });
  }

  loadPosts(): void {
    this.loading.set(true);
    this.postService.getPaged(this.currentPage(), 15, this.selectedCategoryId()).subscribe({
      next: r => { this.result.set(r); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  selectCategory(id?: number): void {
    this.router.navigate([], {
      queryParams: { categoryId: id ?? null, page: null },
      queryParamsHandling: 'merge',
    });
  }

  goToPage(page: number): void {
    this.router.navigate([], { queryParams: { page }, queryParamsHandling: 'merge' });
  }
}
