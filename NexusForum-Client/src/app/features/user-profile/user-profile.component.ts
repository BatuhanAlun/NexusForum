import { Component, Input, OnChanges, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { UserProfileDto } from '../../core/models/user.models';
import { UserService } from '../../core/services/user.service';
import { timeAgo } from '../../core/utils/category.utils';

const ROLE_BADGE: Record<string, string> = {
  Admin: 'red',
  Moderator: 'orange',
  Member: 'blue',
};

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [RouterLink],
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

        <div class="profile-cta">
          <a routerLink="/" class="btn btn-secondary">← View Feed</a>
        </div>
      }
    </div>
  `,
  styles: [`
    .profile-page { max-width: 600px; margin: 0 auto; padding: 2.5rem 1.5rem; }

    .profile-header {
      display: flex; align-items: center; gap: 1.25rem;
      background: var(--bg-surface); border: 1px solid var(--border);
      border-radius: var(--radius-lg); padding: 1.75rem; margin-bottom: 1rem;
    }
    .profile-info { flex: 1; }
    .profile-name-row { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 0.25rem; }
    .profile-username { font-size: 1.5rem; font-weight: 700; }
    .profile-joined { font-size: 0.875rem; color: var(--fg-muted); }

    .stats-grid {
      display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem; margin-bottom: 1.5rem;
    }
    .stat-card {
      background: var(--bg-surface); border: 1px solid var(--border);
      border-radius: var(--radius-lg); padding: 1.25rem;
      display: flex; flex-direction: column; align-items: center; gap: 0.25rem;
    }
    .stat-value { font-size: 1.75rem; font-weight: 700; color: var(--accent); }
    .stat-label { font-size: 0.8125rem; color: var(--fg-muted); }

    .profile-cta { display: flex; justify-content: flex-start; }
  `],
})
export class UserProfileComponent implements OnChanges {
  private userService = inject(UserService);
  protected ago = timeAgo;

  @Input() username!: string;

  profile = signal<UserProfileDto | null>(null);
  loading = signal(true);

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
}
