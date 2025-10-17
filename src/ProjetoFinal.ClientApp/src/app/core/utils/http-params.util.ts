import { HttpParams } from '@angular/common/http';

export function toHttpParams(source: Record<string, unknown> = {}): HttpParams {
  let params = new HttpParams();

  for (const [key, value] of Object.entries(source)) {
    if (value === null || value === undefined || value === '') {
      continue;
    }

    if (Array.isArray(value)) {
      value.forEach(item => {
        if (item !== null && item !== undefined && item !== '') {
          params = params.append(key, String(item));
        }
      });
      continue;
    }

    params = params.set(key, String(value));
  }

  return params;
}
