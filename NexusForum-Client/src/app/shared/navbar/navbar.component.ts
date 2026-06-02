import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar">
      <div class="nav-inner">
        <a routerLink="/" class="brand">
          <span class="brand-icon">&lt;/&gt;</span>
          <span class="brand-name">NexusForum</span>
        </a>

        <div class="nav-links">
          <a routerLink="/" [routerLinkActiveOptions]="{exact:true}" routerLinkActive="active" class="nav-link">
            Feed
          </a>
          <a routerLink="/" [queryParams]="{categoryId: 2}" class="nav-link">Tech</a>
          <a routerLink="/" [queryParams]="{categoryId: 3}" class="nav-link">Security</a>
        </div>

        <div class="nav-actions">
          @if (auth.isLoggedIn()) {
            <a routerLink="/posts/new" class="btn btn-primary btn-sm">+ New Post</a>
            @if (auth.isAdmin()) {
              <a routerLink="/pentester" routerLinkActive="active" class="btn btn-ghost btn-sm pentester-btn" title="Run pentest">
                🛡 Pentester
              </a>
            }
            <a [routerLink]="['/users', auth.user()?.username]" class="user-chip">
              <span class="chip-avatar">{{ auth.user()?.username?.[0]?.toUpperCase() }}</span>
              <span class="chip-name">{{ auth.user()?.username }}</span>
            </a>
            <button class="btn btn-ghost btn-sm" (click)="auth.logout()">Sign out</button>
          } @else {
            <a routerLink="/login" class="btn btn-ghost btn-sm">Sign in</a>
            <a routerLink="/register" class="btn btn-primary btn-sm">Register</a>
          }
        </div>
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      position: sticky; top: 0; z-index: 100;
      background: rgba(22, 27, 34, 0.95);
      border-bottom: 1px solid var(--border);
      backdrop-filter: blur(12px);
    }
    .nav-inner {
      max-width: 1200px; margin: 0 auto; padding: 0 1.5rem;
      height: 56px; display: flex; align-items: center; gap: 1.5rem;
    }
    .brand {
      display: flex; align-items: center; gap: 0.5rem;
      text-decoration: none !important; flex-shrink: 0;
    }
    .brand-icon {
      font-family: var(--font-mono); font-size: 0.9rem; font-weight: 700;
      color: var(--accent); letter-spacing: -0.05em;
    }
    .brand-name {
      font-size: 1rem; font-weight: 700; color: var(--fg-default); letter-spacing: -0.02em;
    }
    .nav-links {
      display: flex; align-items: center; gap: 0.125rem; flex: 1;
    }
    .nav-link {
      padding: 0.375rem 0.75rem; border-radius: var(--radius);
      font-size: 0.875rem; font-weight: 500; color: var(--fg-muted);
      text-decoration: none !important; transition: all 0.15s;
    }
    .nav-link:hover, .nav-link.active {
      color: var(--fg-default); background: var(--bg-elevated);
    }
    .nav-actions { display: flex; align-items: center; gap: 0.625rem; flex-shrink: 0; }
    .user-chip {
      display: flex; align-items: center; gap: 0.4rem;
      padding: 0.2rem 0.625rem 0.2rem 0.2rem;
      border-radius: 100px; background: var(--bg-elevated);
      border: 1px solid var(--border); text-decoration: none !important;
      color: var(--fg-default); font-size: 0.8125rem; font-weight: 500;
      transition: border-color 0.15s;
    }
    .user-chip:hover { border-color: var(--accent); color: var(--fg-default); }
    .pentester-btn {
      border: 1px solid var(--accent);
      color: var(--accent);
    }
    .pentester-btn:hover, .pentester-btn.active {
      background: var(--accent);
      color: #fff;
    }
    .chip-avatar {
      width: 22px; height: 22px; border-radius: 50%;
      background: linear-gradient(135deg, var(--accent) 0%, var(--purple) 100%);
      display: flex; align-items: center; justify-content: center;
      font-size: 0.6rem; font-weight: 700; color: #fff;
    }
  `],
})
export class NavbarComponent {
  auth = inject(AuthService);
}
