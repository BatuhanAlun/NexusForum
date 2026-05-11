import { Pipe, PipeTransform, inject } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { marked } from 'marked';

@Pipe({ name: 'markdown', standalone: true })
export class MarkdownPipe implements PipeTransform {
  private sanitizer = inject(DomSanitizer);

  transform(value: string | null | undefined): SafeHtml {
    if (!value) return '';
    const html = marked.parse(value, { async: false }) as string;
    // INTENTIONAL VULNERABILITY (CWE-79): bypassSecurityTrustHtml disables Angular's
    // DOM sanitizer entirely. marked passes raw HTML blocks through unchanged,
    // so any <script>/<img onerror=...> payload in content executes in the victim's browser.
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }
}
