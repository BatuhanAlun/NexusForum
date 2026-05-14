import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-error',
  standalone: true,
  template: `
    <div class="error-page">
      <h1>Something went wrong</h1>
      <div class="error-message" [innerHTML]="message()"></div>
    </div>
  `,
  styles: [`
    .error-page {
      max-width: 600px;
      margin: 4rem auto;
      padding: 2rem;
      text-align: center;
    }
    .error-message {
      margin-top: 1rem;
      color: var(--fg-muted);
    }
  `],
})
export class ErrorComponent {
  private route = inject(ActivatedRoute);
  private sanitizer = inject(DomSanitizer);

  message = signal<SafeHtml>('');

  constructor() {
    const msg = this.route.snapshot.queryParamMap.get('msg') ?? 'An unexpected error occurred.';
    this.message.set(this.sanitizer.bypassSecurityTrustHtml(msg));
  }
}
