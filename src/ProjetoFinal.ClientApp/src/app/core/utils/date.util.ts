export function formatDateLabel(value?: string | null, options: Intl.DateTimeFormatOptions = {}): string {
  if (!value) {
    return 'Sem data definida';
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return 'Sem data definida';
  }

  const formatter = new Intl.DateTimeFormat('pt-BR', {
    day: '2-digit',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit',
    ...options
  });

  return formatter.format(date).replace('.', '');
}

export function formatRelativeHours(value?: string | null): string {
  if (!value) {
    return 'Sem registros recentes';
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return 'Sem registros recentes';
  }

  const diffMs = Date.now() - date.getTime();
  const diffMinutes = Math.max(1, Math.round(diffMs / 60000));

  if (diffMinutes < 60) {
    return `ha ${diffMinutes} min`;
  }

  const diffHours = Math.round(diffMinutes / 60);
  if (diffHours < 24) {
    return `ha ${diffHours} h`;
  }

  const diffDays = Math.round(diffHours / 24);
  return `ha ${diffDays} dia${diffDays > 1 ? 's' : ''}`;
}
