import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, of, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  ForumPostCreatePayload,
  ForumPostDto,
  ForumPostFilter
} from '../api/forum.api';
import { ApiPagedResponse, normalizePagedResponse } from '../api/api.types';
import { toHttpParams } from '../utils/http-params.util';

@Injectable({ providedIn: 'root' })
export class ForumPostsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.baseUrl;

  getPosts(filter: ForumPostFilter = {}) {
    const params = toHttpParams({ PageSize: 200, PageNumber: 1, ...filter });

    return this.http
      .get<ApiPagedResponse<ForumPostDto>>(`${this.baseUrl}/forum/posts`, { params })
      .pipe(
        catchError(() => of<ApiPagedResponse<ForumPostDto>>({ dados: [] })),
        map(response => normalizePagedResponse(response).items)
      );
  }

  createPost(payload: ForumPostCreatePayload) {
    return this.http
      .post<ForumPostDto>(`${this.baseUrl}/forum/posts`, payload)
      .pipe(catchError(error => throwError(() => error)));
  }
}
