import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { MediaKind, MediaResource } from '../api/media.api';

@Injectable({ providedIn: 'root' })
export class MediaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.baseUrl;

  upload(file: File, kind?: MediaKind) {
    const resolvedKind = kind ?? inferMediaKind(file);
    const formData = new FormData();
    formData.append('file', file, file.name);
    formData.append('kind', String(resolvedKind));

    return this.http
      .post<MediaResource>(`${this.baseUrl}/media-resources/upload`, formData)
      .pipe(catchError(error => throwError(() => error)));
  }
}

export function inferMediaKind(file: File): MediaKind {
  const contentType = file.type?.toLowerCase() ?? '';
  if (contentType.startsWith('image/')) {
    return MediaKind.Image;
  }
  if (contentType.startsWith('video/')) {
    return MediaKind.Video;
  }
  if (contentType.startsWith('audio/')) {
    return MediaKind.Audio;
  }

  const extension = file.name.split('.').pop()?.toLowerCase() ?? '';
  if (['png', 'jpg', 'jpeg', 'gif', 'webp', 'heic', 'bmp'].includes(extension)) {
    return MediaKind.Image;
  }
  if (['mp4', 'mov', 'mkv', 'webm', 'avi'].includes(extension)) {
    return MediaKind.Video;
  }
  if (['mp3', 'wav', 'ogg', 'aac', 'flac'].includes(extension)) {
    return MediaKind.Audio;
  }

  return MediaKind.Document;
}
