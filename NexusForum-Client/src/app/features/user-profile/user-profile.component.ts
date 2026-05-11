import { Component, Input, OnChanges, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { UpdateProfileRequest, UserProfileDto } from '../../core/models/user.models';
import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../../core/services/user.service';
import { apiError, timeAgo } from '../../core/utils/category.utils';

const ROLE_BADGE: Record<string, string> = {
  Admin: 'red',
  Moderator: 'orange',
  Member: 'blue',
};

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [RouterLink, FormsModule],
  template: `
    <div class="profile-page">
      @if (loading()) {
        <div class="loading-wrap">Loading profile</div>
      } @else if (!profile()) {
        <div class="empty-state">
          <h3>User not found</h3>
          <p>This user doesn't exist or may have been removed.</p>
          <a routerLink="/" class="btn btn-secondary" style="margin-top:1rem">← Back to Feed</a>
        </div>
      } @else {
        <div class="profile-header">
          <div class="avatar-lg">{{ profile()!.username[0].toUpperCase() }}</div>
          <div class="profile-info">
            <div class="profile-name-row">
              <h1 class="profile-username">{{ profile()!.username }}</h1>
              <span class="badge badge-{{ roleBadge(profile()!.role) }}">{{ profile()!.role }}</span>
            </div>
            @if (profile()!.bio) {
              <p class="profile-bio">{{ profile()!.bio }}</p>
            }
            <p class="profile-joined">Member since {{ ago(profile()!.createdAt) }}</p>
          </div>
        </div>

        <div class="stats-grid">
          <div class="stat-card">
            <span class="stat-value">{{ profile()!.postCount }}</span>
            <span class="stat-label">Posts</span>
          </div>
          <div class="stat-card">
            <span class="stat-value">{{ profile()!.commentCount }}</span>
            <span class="stat-label">Replies</span>
          </div>
        </div>

        @if (isOwnProfile()) {
          <div class="edit-section">
            @if (!editing()) {
              <button class="btn btn-secondary btn-sm" (click)="startEdit()">Edit Profile</button>
            } @else {
              <div class="edit-card">
                <h3 class="edit-heading">Edit Profile</h3>

                @if (editError()) {
                  <div class="alert alert-error">{{ editError() }}</div>
                }

                <div class="form-group">
                  <label for="ep-username">Username</label>
                  <input id="ep-username" type="text" [(ngModel)]="editUsername" />
                </div>
                <div class="form-group">
                  <label for="ep-bio">Bio</label>
                  <textarea id="ep-bio" [(ngModel)]="editBio" rows="3" placeholder="Tell the community about yourself…"></textarea>
                </div>

                <div class="edit-actions">
                  <button class="btn btn-primary btn-sm" (click)="saveEdit()" [disabled]="saving()">
                    {{ saving() ? 'Saving…' : 'Save' }}
                  </button>
                  <button class="btn btn-ghost btn-sm" (click)="cancelEdit()">Cancel</button>
                </div>
              </div>
            }
          </div>
        }

        <div class="profile-cta">
          <a routerLink="/" class="btn btn-secondary">← View Feed</a>
        </div>
      }
    </div>
  `,
  styles: [`
    .profile-page { max-width: 600px; margin: 0 auto; padding: 2.5rem 1.5rem; }

    .profile-header {
      display: flex; align-items: flex-start; gap: 1.25rem;
      background: var(--bg-surface); border: 1px solid var(--border);
      border-radius: var(--radius-lg); padding: 1.75rem; margin-bottom: 1rem;
    }
    .profile-info { flex: 1; }
    .profile-name-row { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 0.25rem; }
    .profile-username { font-size: 1.5rem; font-weight: 700; }
    .profile-bio { font-size: 0.9rem; color: var(--fg-muted); margin: 0.25rem 0; }
    .profile-joined { font-size: 0.875rem; color: var(--fg-muted); margin-top: 0.25rem; }

    .stats-grid {
      display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem; margin-bottom: 1rem;
    }
    .stat-card {
      background: var(--bg-surface); border: 1px solid var(--border);
      border-radius: var(--radius-lg); padding: 1.25rem;
      display: flex; flex-direction: column; align-items: center; gap: 0.25rem;
    }
    .stat-value { font-size: 1.75rem; font-weight: 700; color: var(--accent); }
    .stat-label { font-size: 0.8125rem; color: var(--fg-muted); }

    .edit-section { margin-bottom: 1rem; }
    .edit-card {
      background: var(--bg-surface); border: 1px solid var(--border);
      border-radius: var(--radius-lg); padding: 1.5rem;
    }
    .edit-heading { font-size: 1rem; font-weight: 600; margin-bottom: 1rem; }
    .edit-actions { display: flex; gap: 0.5rem; margin-top: 0.75rem; }

    .profile-cta { display: flex; justify-content: flex-start; }
  `],
})
export class UserProfileComponent implements OnChanges {
  private userService = inject(UserService);
  private auth = inject(AuthService);
  protected ago = timeAgo;

  @Input() username!: string;

  profile = signal<UserProfileDto | null>(null);
  loading = signal(true);
  editing = signal(false);
  saving = signal(false);
  editError = signal<string | null>(null);
  editUsername = '';
  editBio = '';

  isOwnProfile = computed(() => {
    const u = this.auth.user();
    const p = this.profile();
    return !!u && !!p && u.username === p.username;
  });

  roleBadge(role: string): string {
    return ROLE_BADGE[role] ?? 'blue';
  }

  ngOnChanges(): void {
    if (this.username) {
      this.loading.set(true);
      this.userService.getProfile(this.username).subscribe({
        next: p => { this.profile.set(p); this.loading.set(false); },
        error: () => { this.profile.set(null); this.loading.set(false); },
      });
    }
  }

  startEdit(): void {
    const p = this.profile()!;
    this.editUsername = p.username;
    this.editBio = p.bio ?? '';
    this.editError.set(null);
    this.editing.set(true);
  }

  cancelEdit(): void {
    this.editing.set(false);
  }

  saveEdit(): void {
    if (!this.editUsername.trim()) return;
    this.saving.set(true);
    this.editError.set(null);
    const req: UpdateProfileRequest = {
      username: this.editUsername.trim(),
      bio: this.editBio,
    };
    this.userService.updateProfile(req).subscribe({
      next: updated => {
        this.profile.set(updated);
        this.editing.set(false);
        this.saving.set(false);
      },
      error: (err) => {
        this.editError.set(apiError(err));
        this.saving.set(false);
      },
    });
  }
}
