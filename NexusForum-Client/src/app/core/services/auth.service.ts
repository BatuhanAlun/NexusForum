import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { AuthResponse, AuthUser, LoginRequest, RegisterRequest } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private _user = signal<AuthUser | null>(this.loadStored());
  readonly user = this._user.asReadonly();
  readonly isLoggedIn = computed(() => this._user() !== null);
  readonly isAdmin = computed(() => this._user()?.role === 'Admin');

  private loadStored(): AuthUser | null {
    try {
      const s = localStorage.getItem('nexus_user');
      return s ? (JSON.parse(s) as AuthUser) : null;
    } catch {
      return null;
    }
  }

  login(req: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/login', req).pipe(tap(r => this.persist(r)));
  }

  register(req: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/register', req).pipe(tap(r => this.persist(r)));
  }

  logout(): void {
    this.http.post('/api/auth/logout', {}).subscribe();
    this._user.set(null);
    localStorage.removeItem('nexus_user');
    this.router.navigate(['/']);
  }

  getToken(): string | null {
    return this._user()?.token ?? null;
  }

  private persist(r: AuthResponse): void {
    const u: AuthUser = { id: r.id, username: r.username, email: r.email, role: r.role, token: r.token };
    this._user.set(u);
    localStorage.setItem('nexus_user', JSON.stringify(u));
  }
}
