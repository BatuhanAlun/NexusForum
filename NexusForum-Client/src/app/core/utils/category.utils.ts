const COLOR_MAP: Record<string, string> = {
  'General': 'blue',
  'Technology': 'green',
  'Security': 'red',
  'Off Topic': 'purple',
};

export function categoryColor(name: string): string {
  return COLOR_MAP[name] ?? 'blue';
}

export function timeAgo(dateString: string): string {
  const diff = Math.floor((Date.now() - new Date(dateString).getTime()) / 1000);
  if (diff < 60) return 'just now';
  if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
  if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
  if (diff < 604800) return `${Math.floor(diff / 86400)}d ago`;
  return new Date(dateString).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

export function apiError(err: unknown): string {
  if (!err || typeof err !== 'object' || !('error' in err)) return 'Something went wrong.';
  const body = (err as { error?: Record<string, unknown> }).error;
  if (!body) return 'Something went wrong.';

  // ValidationProblem: { errors: { FieldName: string[] } }
  if (body['errors'] && typeof body['errors'] === 'object') {
    const msgs = Object.values(body['errors'] as Record<string, string[]>).flat();
    if (msgs.length) return msgs.join(' ');
  }

  // Problem: { detail: string } or { title: string }
  return (body['detail'] as string) || (body['title'] as string) || 'Something went wrong.';
}
