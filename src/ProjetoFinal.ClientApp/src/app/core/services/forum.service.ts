import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, of } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ForumThreadDto, ForumThreadListItem, mapForumThreadsResponse } from '../api/forum.api';
import { ApiPagedResponse } from '../api/api.types';
import { toHttpParams } from '../utils/http-params.util';

@Injectable({ providedIn: 'root' })
export class ForumService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.baseUrl;

  getThreads(courseLookup: Map<string, string>) {
    const params = toHttpParams({ PageSize: 20, PageNumber: 1 });

    return this.http
      .get<ApiPagedResponse<ForumThreadDto>>(`${this.baseUrl}/forum/threads`, { params })
      .pipe(
        catchError(() => of<ApiPagedResponse<ForumThreadDto>>({ dados: [] })),
        map(response => mapForumThreadsResponse(response, courseLookup))
      );
  }
}
