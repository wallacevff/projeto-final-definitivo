import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, of, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  ContentVideoAnnotationCreatePayload,
  ContentVideoAnnotationDto,
  ContentVideoAnnotationFilter,
  mapContentAnnotationsResponse
} from '../api/content-annotations.api';
import { ApiPagedResponse } from '../api/api.types';
import { toHttpParams } from '../utils/http-params.util';

@Injectable({ providedIn: 'root' })
export class ContentAnnotationsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.baseUrl;

  getAnnotations(filter: ContentVideoAnnotationFilter) {
    const params = toHttpParams({ PageSize: 200, PageNumber: 1, ...filter });
    return this.http
      .get<ApiPagedResponse<ContentVideoAnnotationDto>>(`${this.baseUrl}/content-annotations`, { params })
      .pipe(
        catchError(() => of<ApiPagedResponse<ContentVideoAnnotationDto>>({ dados: [] })),
        map(mapContentAnnotationsResponse)
      );
  }

  addAnnotation(payload: ContentVideoAnnotationCreatePayload) {
    return this.http
      .post<ContentVideoAnnotationDto>(`${this.baseUrl}/content-annotations`, payload)
      .pipe(catchError(error => throwError(() => error)));
  }
}
