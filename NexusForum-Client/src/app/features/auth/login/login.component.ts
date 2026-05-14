import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { apiError } from '../../../core/utils/category.utils';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-card">
        <div class="auth-header">
          <span class="auth-logo">&lt;/&gt;</span>
          <h1>Welcome back</h1>
          <p>Sign in to your NexusForum account</p>
        </div>

        @if (error()) {
          <div class="alert alert-error">{{ error() }}</div>
        }

        <form [formGroup]="form" (ngSubmit)="submit()" class="auth-form">
          <div class="form-group">
            <label for="email">Email</label>
            <input id="email" type="email" formControlName="email" placeholder="you@example.com" autocomplete="email" />
            @if (form.get('email')?.invalid && form.get('email')?.touched) {
              <span class="field-error">Valid email required</span>
            }
          </div>

          <div class="form-group">
            <label for="password">Password</label>
            <input id="password" type="password" formControlName="password" placeholder="••••••••" autocomplete="current-password" />
            @if (form.get('password')?.invalid && form.get('password')?.touched) {
              <span class="field-error">Password required</span>
            }
          </div>

          <button type="submit" class="btn btn-primary btn-full" [disabled]="loading()">
            {{ loading() ? 'Signing in…' : 'Sign in' }}
          </button>
        </form>

        <p class="auth-footer">
          Don't have an account? <a routerLink="/register">Register</a>
        </p>
      </div>
    </div>
  `,
  styles: [`
    .auth-page {
      min-height: calc(100vh - 56px);
      display: flex; align-items: center; justify-content: center;
      padding: 2rem 1rem;
    }
    .auth-card {
      width: 100%; max-width: 400px;
      background: var(--bg-surface); border: 1px solid var(--border);
      border-radius: var(--radius-xl); padding: 2rem;
    }
    .auth-header { text-align: center; margin-bottom: 1.5rem; }
    .auth-logo {
      font-family: var(--font-mono); font-size: 1.5rem;
      font-weight: 700; color: var(--accent); display: block; margin-bottom: 1rem;
    }
    .auth-header h1 { font-size: 1.375rem; font-weight: 700; color: var(--fg-default); }
    .auth-header p { font-size: 0.875rem; color: var(--fg-muted); margin-top: 0.25rem; }
    .auth-form { display: flex; flex-direction: column; gap: 1rem; }
    .auth-footer { text-align: center; font-size: 0.875rem; color: var(--fg-muted); margin-top: 1.25rem; }
  `],
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  loading = signal(false);
  error = signal<string | null>(null);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    this.error.set(null);
    const { email, password } = this.form.value;
    this.auth.login({ email: email!, password: password! }).subscribe({
      next: () => {
        const redirect = this.route.snapshot.queryParamMap.get('redirect');
        if (redirect) {
          window.location.href = redirect;
        } else {
          this.router.navigate(['/']);
        }
      },
      error: (err) => { this.error.set(apiError(err)); this.loading.set(false); },
    });
  }
}
