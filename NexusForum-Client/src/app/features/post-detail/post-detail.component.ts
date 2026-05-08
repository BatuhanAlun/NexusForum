import { Component, Input, OnChanges, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommentDto } from '../../core/models/comment.models';
import { PostDto, PostMemberDto } from '../../core/models/post.models';
import { AuthService } from '../../core/services/auth.service';
import { CommentService } from '../../core/services/comment.service';
import { PostService } from '../../core/services/post.service';
import { apiError, categoryColor, timeAgo } from '../../core/utils/category.utils';

@Component({
  selector: 'app-post-detail',
  standalone: true,
  imports: [RouterLink, FormsModule],
  template: `
    <div class="detail-page">
      @if (loading()) {
        <div class="loading-wrap">Loading post</div>
      } @else if (!post()) {
        <div class="empty-state">
          <h3>Post not found</h3>
          <p>This post may have been deleted or is private.</p>
          <a routerLink="/" class="btn btn-secondary" style="margin-top:1rem">← Back to Feed</a>
        </div>
      } @else {
        <div class="post-container">
          <a routerLink="/" class="back-link">← Back to Feed</a>

          <article class="post-body">
            <div class="post-category-row">
              <span class="badge badge-{{ color(post()!.categoryName) }}">{{ post()!.categoryName }}</span>
              @if (post()!.isPrivate) {
                <span class="badge badge-private">🔒 Private</span>
              }
            </div>
            <h1 class="post-h1">{{ post()!.title }}</h1>
            <div class="post-meta-row">
              <a [routerLink]="['/users', post()!.authorUsername]" class="meta-author">
                <span class="avatar-sm">{{ post()!.authorUsername[0].toUpperCase() }}</span>
                <strong>{{ post()!.authorUsername }}</strong>
              </a>
              <span class="meta-sep">·</span>
              <span class="meta-time">{{ ago(post()!.createdAt) }}</span>
              @if (post()!.updatedAt) {
                <span class="meta-edited">(edited)</span>
              }
              @if (canManagePost()) {
                <div class="post-actions">
                  <a [routerLink]="['/posts', post()!.id, 'edit']" class="btn btn-ghost btn-sm">Edit</a>
                  <button class="btn btn-danger btn-sm" (click)="deletePost()">Delete</button>
                </div>
              }
            </div>
            @if (post()!.isRestricted) {
              <div class="restricted-body">
                <div class="restricted-blur">
                  Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.
                  Excepteur sint occaecat cupidatat non proident sunt in culpa qui officia deserunt mollit anim id est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium totam rem aperiam eaque ipsa quae ab illo inventore veritatis.
                </div>
                <div class="restricted-overlay">
                  <div class="restricted-lock">🔒</div>
                  <p class="restricted-title">This is a private thread</p>
                  <p class="restricted-hint">Only invited members can read this post. Contact the author to request access.</p>
                </div>
              </div>
            } @else {
              <div class="post-content">{{ post()!.content }}</div>
            }
          </article>

          @if (post()!.isPrivate && isAuthor()) {
            <section class="members-section">
              <h2 class="section-heading">Members</h2>

              @if (memberError()) {
                <div class="alert alert-error">{{ memberError() }}</div>
              }

              <div class="member-list">
                <div class="member-row author-row">
                  <span class="avatar-sm">{{ post()!.authorUsername[0].toUpperCase() }}</span>
                  <span class="member-name">{{ post()!.authorUsername }}</span>
                  <span class="member-role">Author</span>
                </div>
                @for (m of post()!.members; track m.userId) {
                  <div class="member-row">
                    <span class="avatar-sm">{{ m.username[0].toUpperCase() }}</span>
                    <span class="member-name">{{ m.username }}</span>
                    <span class="member-since">invited {{ ago(m.invitedAt) }}</span>
                    <button class="btn btn-danger btn-sm" (click)="removeMember(m)">Remove</button>
                  </div>
                }
              </div>

              <form class="invite-form" (ngSubmit)="invite()">
                <input
                  type="text"
                  [(ngModel)]="inviteUsername"
                  name="inviteUsername"
                  placeholder="Username to invite…"
                  class="invite-input"
                />
                <button type="submit" class="btn btn-primary btn-sm" [disabled]="inviting()">
                  {{ inviting() ? 'Inviting…' : 'Invite' }}
                </button>
              </form>
            </section>
          }

          @if (!post()!.isRestricted) {
          <section class="comments-section">
            <h2 class="comments-heading">
              {{ post()!.comments.length }} {{ post()!.comments.length === 1 ? 'Reply' : 'Replies' }}
            </h2>

            @if (post()!.comments.length === 0) {
              <p class="no-comments">No replies yet — be the first!</p>
            }

            @for (comment of post()!.comments; track comment.id) {
              <div class="comment">
                <span class="avatar-sm">{{ comment.authorUsername[0].toUpperCase() }}</span>
                <div class="comment-body">
                  <div class="comment-header">
                    <a [routerLink]="['/users', comment.authorUsername]" class="comment-author">{{ comment.authorUsername }}</a>
                    <span class="comment-time">{{ ago(comment.createdAt) }}</span>
                    @if (canManageComment(comment)) {
                      <div class="comment-actions">
                        <button class="btn btn-ghost btn-sm" (click)="startEdit(comment)">Edit</button>
                        <button class="btn btn-danger btn-sm" (click)="deleteComment(comment.id)">Delete</button>
                      </div>
                    }
                  </div>
                  @if (editingId() === comment.id) {
                    <div class="edit-wrap">
                      <textarea class="edit-textarea" [(ngModel)]="editContent" rows="3"></textarea>
                      <div class="edit-btns">
                        <button class="btn btn-primary btn-sm" (click)="saveEdit(comment.id)">Save</button>
                        <button class="btn btn-ghost btn-sm" (click)="cancelEdit()">Cancel</button>
                      </div>
                    </div>
                  } @else {
                    <p class="comment-content">{{ comment.content }}</p>
                  }
                </div>
              </div>
            }

            @if (auth.isLoggedIn()) {
              <div class="reply-box">
                <span class="avatar-sm">{{ auth.user()?.username?.[0]?.toUpperCase() }}</span>
                <div class="reply-input">
                  @if (commentError()) {
                    <div class="alert alert-error">{{ commentError() }}</div>
                  }
                  <textarea [(ngModel)]="newComment" placeholder="Write a reply…" rows="3" class="reply-textarea"></textarea>
                  <button class="btn btn-primary btn-sm" (click)="submitComment()" [disabled]="submitting()">
                    {{ submitting() ? 'Posting…' : 'Post Reply' }}
                  </button>
                </div>
              </div>
            } @else {
              <p class="login-prompt">
                <a routerLink="/login">Sign in</a> to join the discussion.
              </p>
            }
          </section>
          } <!-- end @if !isRestricted -->
        </div>
      }
    </div>
  `,
  styles: [`
    .detail-page { max-width: 860px; margin: 0 auto; padding: 2rem 1.5rem; }
    .back-link {
      display: inline-flex; align-items: center; gap: 0.25rem;
      font-size: 0.875rem; color: var(--fg-muted); margin-bottom: 1.5rem;
      text-decoration: none !important; transition: color 0.15s;
    }
    .back-link:hover { color: var(--accent); }

    .post-body {
      background: var(--bg-surface); border: 1px solid var(--border);
      border-radius: var(--radius-lg); padding: 1.75rem; margin-bottom: 1.5rem;
    }
    .post-category-row { display: flex; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 0.75rem; }
    .badge-private { background: rgba(188,140,255,.12); color: #bc8cff; border: 1px solid rgba(188,140,255,.3); }
    .post-h1 { font-size: 1.625rem; font-weight: 700; line-height: 1.3; margin-bottom: 0.75rem; }
    .post-meta-row {
      display: flex; align-items: center; flex-wrap: wrap; gap: 0.5rem;
      margin-bottom: 1.5rem; font-size: 0.875rem; color: var(--fg-muted);
    }
    .meta-author {
      display: flex; align-items: center; gap: 0.375rem;
      text-decoration: none !important; color: var(--fg-default); font-weight: 500;
    }
    .meta-author:hover { color: var(--accent); }
    .meta-sep { color: var(--fg-subtle); }
    .meta-edited { font-size: 0.8rem; color: var(--fg-subtle); }
    .post-actions { display: flex; gap: 0.5rem; margin-left: auto; }
    .post-content { white-space: pre-wrap; line-height: 1.75; color: var(--fg-default); }

    .restricted-body {
      position: relative; overflow: hidden; border-radius: var(--radius);
      min-height: 180px;
    }
    .restricted-blur {
      line-height: 1.75; color: var(--fg-default);
      filter: blur(6px); user-select: none; pointer-events: none;
      padding-bottom: 1rem;
    }
    .restricted-overlay {
      position: absolute; inset: 0;
      display: flex; flex-direction: column; align-items: center; justify-content: center;
      gap: 0.5rem; text-align: center;
      background: linear-gradient(to bottom, rgba(13,17,23,0) 0%, rgba(13,17,23,.75) 30%, rgba(13,17,23,.95) 100%);
      padding: 1.5rem;
    }
    .restricted-lock { font-size: 2rem; }
    .restricted-title { font-size: 1rem; font-weight: 700; color: var(--fg-default); }
    .restricted-hint { font-size: 0.8125rem; color: var(--fg-muted); max-width: 340px; }

    .members-section {
      background: var(--bg-surface); border: 1px solid var(--border);
      border-radius: var(--radius-lg); padding: 1.25rem 1.5rem; margin-bottom: 1.5rem;
    }
    .section-heading { font-size: 0.9375rem; font-weight: 600; margin-bottom: 0.875rem; }
    .member-list { display: flex; flex-direction: column; gap: 0.5rem; margin-bottom: 1rem; }
    .member-row {
      display: flex; align-items: center; gap: 0.625rem;
      padding: 0.5rem 0.625rem; border-radius: var(--radius);
      background: var(--bg-elevated);
    }
    .author-row { opacity: 0.8; }
    .member-name { font-size: 0.875rem; font-weight: 500; flex: 1; }
    .member-role { font-size: 0.75rem; color: var(--accent); font-weight: 500; }
    .member-since { font-size: 0.75rem; color: var(--fg-subtle); flex: 1; }
    .invite-form { display: flex; gap: 0.5rem; }
    .invite-input {
      flex: 1; padding: 0.5rem 0.75rem;
      background: var(--bg-elevated); border: 1px solid var(--border);
      border-radius: var(--radius); color: var(--fg-default); font-size: 0.875rem;
    }
    .invite-input:focus { outline: none; border-color: var(--accent); }
    .invite-input::placeholder { color: var(--fg-subtle); }

    .comments-section { display: flex; flex-direction: column; gap: 0; }
    .comments-heading { font-size: 1rem; font-weight: 600; margin-bottom: 1rem; }
    .no-comments { color: var(--fg-muted); font-size: 0.875rem; margin-bottom: 1rem; }

    .comment {
      display: flex; gap: 0.75rem; padding: 1rem 0;
      border-top: 1px solid var(--border-subtle);
    }
    .comment-body { flex: 1; min-width: 0; }
    .comment-header {
      display: flex; align-items: center; gap: 0.5rem;
      margin-bottom: 0.375rem; flex-wrap: wrap;
    }
    .comment-author {
      font-size: 0.875rem; font-weight: 600; color: var(--fg-default);
      text-decoration: none !important;
    }
    .comment-author:hover { color: var(--accent); }
    .comment-time { font-size: 0.8rem; color: var(--fg-subtle); }
    .comment-actions { display: flex; gap: 0.25rem; margin-left: auto; }
    .comment-content { white-space: pre-wrap; line-height: 1.6; font-size: 0.9375rem; }

    .edit-wrap { display: flex; flex-direction: column; gap: 0.5rem; }
    .edit-textarea {
      width: 100%; padding: 0.5rem 0.75rem;
      background: var(--bg-elevated); border: 1px solid var(--border);
      border-radius: var(--radius); color: var(--fg-default); font-size: 0.9rem;
      resize: vertical;
    }
    .edit-textarea:focus { outline: none; border-color: var(--accent); }
    .edit-btns { display: flex; gap: 0.5rem; }

    .reply-box {
      display: flex; gap: 0.75rem; padding-top: 1.25rem;
      border-top: 1px solid var(--border-subtle); margin-top: 0.5rem;
    }
    .reply-input { flex: 1; display: flex; flex-direction: column; gap: 0.5rem; }
    .reply-textarea {
      width: 100%; padding: 0.625rem 0.875rem;
      background: var(--bg-elevated); border: 1px solid var(--border);
      border-radius: var(--radius); color: var(--fg-default); font-size: 0.9rem;
      resize: vertical;
    }
    .reply-textarea:focus { outline: none; border-color: var(--accent); box-shadow: 0 0 0 3px rgba(88,166,255,.1); }
    .reply-textarea::placeholder { color: var(--fg-subtle); }

    .login-prompt {
      padding: 1.25rem 0; border-top: 1px solid var(--border-subtle);
      font-size: 0.875rem; color: var(--fg-muted); margin-top: 0.5rem;
    }
    .post-container { display: flex; flex-direction: column; }
  `],
})
export class PostDetailComponent implements OnChanges {
  private postService = inject(PostService);
  private commentService = inject(CommentService);
  private router = inject(Router);
  protected auth = inject(AuthService);
  protected color = categoryColor;
  protected ago = timeAgo;

  @Input() id!: string;

  post = signal<PostDto | null>(null);
  loading = signal(true);
  editingId = signal<number | null>(null);
  editContent = '';
  newComment = '';
  submitting = signal(false);
  commentError = signal<string | null>(null);
  inviteUsername = '';
  inviting = signal(false);
  memberError = signal<string | null>(null);

  ngOnChanges(): void {
    if (this.id) this.loadPost(Number(this.id));
  }

  loadPost(id: number): void {
    this.loading.set(true);
    this.postService.getById(id).subscribe({
      next: p => { this.post.set(p); this.loading.set(false); },
      error: () => { this.post.set(null); this.loading.set(false); },
    });
  }

  isAuthor(): boolean {
    const user = this.auth.user();
    const post = this.post();
    return !!user && !!post && user.id === post.authorId;
  }

  canManagePost(): boolean {
    const user = this.auth.user();
    const post = this.post();
    return !!user && !!post && (user.id === post.authorId || user.role === 'Admin');
  }

  canManageComment(comment: CommentDto): boolean {
    const user = this.auth.user();
    return !!user && (user.id === comment.authorId || user.role === 'Admin');
  }

  deletePost(): void {
    if (!confirm('Delete this post?')) return;
    this.postService.delete(this.post()!.id).subscribe({
      next: () => this.router.navigate(['/']),
    });
  }

  invite(): void {
    const username = this.inviteUsername.trim();
    if (!username) return;
    this.inviting.set(true);
    this.memberError.set(null);
    this.postService.invite(this.post()!.id, { username }).subscribe({
      next: member => {
        this.post.update(p => p ? { ...p, members: [...p.members, member] } : p);
        this.inviteUsername = '';
        this.inviting.set(false);
      },
      error: (err) => {
        this.memberError.set(apiError(err));
        this.inviting.set(false);
      },
    });
  }

  removeMember(member: PostMemberDto): void {
    if (!confirm(`Remove ${member.username}?`)) return;
    this.postService.removeMember(this.post()!.id, member.userId).subscribe({
      next: () => {
        this.post.update(p => p ? { ...p, members: p.members.filter(m => m.userId !== member.userId) } : p);
      },
      error: (err) => this.memberError.set(apiError(err)),
    });
  }

  startEdit(comment: CommentDto): void {
    this.editingId.set(comment.id);
    this.editContent = comment.content;
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.editContent = '';
  }

  saveEdit(commentId: number): void {
    if (!this.editContent.trim()) return;
    this.commentService.update(commentId, { content: this.editContent }).subscribe({
      next: updated => {
        this.post.update(p => p ? {
          ...p,
          comments: p.comments.map(c => c.id === commentId ? updated : c),
        } : p);
        this.cancelEdit();
      },
    });
  }

  deleteComment(commentId: number): void {
    if (!confirm('Delete this reply?')) return;
    this.commentService.delete(commentId).subscribe({
      next: () => {
        this.post.update(p => p ? {
          ...p,
          comments: p.comments.filter(c => c.id !== commentId),
        } : p);
      },
    });
  }

  submitComment(): void {
    if (!this.newComment.trim()) return;
    this.submitting.set(true);
    this.commentError.set(null);
    this.commentService.create(this.post()!.id, { content: this.newComment }).subscribe({
      next: comment => {
        this.post.update(p => p ? { ...p, comments: [...p.comments, comment] } : p);
        this.newComment = '';
        this.submitting.set(false);
      },
      error: (err) => {
        this.commentError.set(apiError(err));
        this.submitting.set(false);
      },
    });
  }
}
