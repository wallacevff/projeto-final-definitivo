import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, of, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  CourseContentCreatePayload,
  CourseContentDto,
  CourseContentListItem,
  CourseContentsFilter,
  mapCourseContentsResponse
} from '../api/contents.api';
import { ApiPagedResponse } from '../api/api.types';
import { toHttpParams } from '../utils/http-params.util';

@Injectable({ providedIn: 'root' })
export class CourseContentsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.baseUrl;

  getContents(filter: CourseContentsFilter) {
    const params = toHttpParams({ PageSize: 50, ...filter });
    return this.http
      .get<ApiPagedResponse<CourseContentDto>>(`${this.baseUrl}/course-contents`, { params })
      .pipe(
        catchError(() => of<ApiPagedResponse<CourseContentDto>>({ dados: [] })),
        map(mapCourseContentsResponse)
      );
  }

  getContentById(contentId: string) {
    return this.http
      .get<CourseContentDto>(`${this.baseUrl}/course-contents/${contentId}`)
      .pipe(catchError(error => throwError(() => error)));
  }

  createContent(payload: CourseContentCreatePayload) {
    return this.http
      .post<CourseContentDto>(`${this.baseUrl}/course-contents`, payload)
      .pipe(catchError(error => throwError(() => error)));
  }

  publishContent(contentId: string) {
    return this.http
      .put<void>(`${this.baseUrl}/course-contents/${contentId}/publish`, {})
      .pipe(catchError(error => throwError(() => error)));
  }
}
