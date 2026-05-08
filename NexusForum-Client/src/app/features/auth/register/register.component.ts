import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { apiError } from '../../../core/utils/category.utils';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-card">
        <div class="auth-header">
          <span class="auth-logo">&lt;/&gt;</span>
          <h1>Create account</h1>
          <p>Join the NexusForum developer community</p>
        </div>

        @if (error()) {
          <div class="alert alert-error">{{ error() }}</div>
        }

        <form [formGroup]="form" (ngSubmit)="submit()" class="auth-form">
          <div class="form-group">
            <label for="username">Username</label>
            <input id="username" type="text" formControlName="username" placeholder="devhero" autocomplete="username" />
            @if (form.get('username')?.invalid && form.get('username')?.touched) {
              <span class="field-error">3–50 characters required</span>
            }
          </div>

          <div class="form-group">
            <label for="email">Email</label>
            <input id="email" type="email" formControlName="email" placeholder="you@example.com" autocomplete="email" />
            @if (form.get('email')?.invalid && form.get('email')?.touched) {
              <span class="field-error">Valid email required</span>
            }
          </div>

          <div class="form-group">
            <label for="password">Password</label>
            <input id="password" type="password" formControlName="password" placeholder="••••••••" autocomplete="new-password" />
            @if (form.get('password')?.invalid && form.get('password')?.touched) {
              <span class="field-error">Minimum 8 characters</span>
            }
          </div>

          <button type="submit" class="btn btn-primary btn-full" [disabled]="loading()">
            {{ loading() ? 'Creating account…' : 'Create account' }}
          </button>
        </form>

        <p class="auth-footer">
          Already have an account? <a routerLink="/login">Sign in</a>
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
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  loading = signal(false);
  error = signal<string | null>(null);

  form = this.fb.group({
    username: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    this.error.set(null);
    const { username, email, password } = this.form.value;
    this.auth.register({ username: username!, email: email!, password: password! }).subscribe({
      next: () => this.router.navigate(['/']),
      error: (err) => { this.error.set(apiError(err)); this.loading.set(false); },
    });
  }
}
