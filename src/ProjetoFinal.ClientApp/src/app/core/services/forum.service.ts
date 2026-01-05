import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, of, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  ForumThreadCreatePayload,
  ForumThreadDto,
  ForumThreadFilter,
  ForumThreadListItem,
  mapForumThreadsResponse
} from '../api/forum.api';
import { ApiPagedResponse } from '../api/api.types';
import { toHttpParams } from '../utils/http-params.util';

@Injectable({ providedIn: 'root' })
export class ForumService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.baseUrl;

  getThreads(courseLookup: Map<string, string>, filter: ForumThreadFilter = {}) {
    const params = toHttpParams({ PageSize: 20, PageNumber: 1, ...filter });

    return this.http
      .get<ApiPagedResponse<ForumThreadDto>>(`${this.baseUrl}/forum/threads`, { params })
      .pipe(
        catchError(() => of<ApiPagedResponse<ForumThreadDto>>({ dados: [] })),
        map(response => mapForumThreadsResponse(response, courseLookup))
      );
  }

  getThreadsByClassGroup(courseLookup: Map<string, string>, classGroupId: string) {
    return this.getThreads(courseLookup, { ClassGroupId: classGroupId });
  }

  getThreadById(threadId: string) {
    return this.http
      .get<ForumThreadDto>(`${this.baseUrl}/forum/threads/${threadId}`)
      .pipe(catchError(error => throwError(() => error)));
  }

  createThread(payload: ForumThreadCreatePayload) {
    return this.http
      .post<ForumThreadDto>(`${this.baseUrl}/forum/threads`, payload)
      .pipe(catchError(error => throwError(() => error)));
  }
}
