import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, of, shareReplay, Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { CourseDto, CoursesFilter, mapClassGroupsFromCourses, mapCoursesResponse } from '../api/courses.api';
import { ApiPagedResponse, normalizePagedResponse } from '../api/api.types';
import { toHttpParams } from '../utils/http-params.util';

@Injectable({ providedIn: 'root' })
export class CoursesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.baseUrl;
  private cache$?: Observable<ApiPagedResponse<CourseDto>>;

  getCourseCards(filter: CoursesFilter = {}) {
    return this.fetchCoursesDto(filter).pipe(map(mapCoursesResponse));
  }

  getCoursesDto() {
    return this.fetchCoursesDto().pipe(map(response => normalizePagedResponse(response).items));
  }

  getClassGroupRows() {
    return this.fetchCoursesDto().pipe(
      map(response => normalizePagedResponse(response).items),
      map(courses => mapClassGroupsFromCourses(courses))
    );
  }

  private fetchCoursesDto(filter: CoursesFilter = {}): Observable<ApiPagedResponse<CourseDto>> {
    if (!this.cache$ || Object.keys(filter).length) {
      const params = toHttpParams({ PageSize: 50, ...filter });
      const request = this.http
        .get<ApiPagedResponse<CourseDto>>(`${this.baseUrl}/courses`, { params })
        .pipe(
          catchError(() => of<ApiPagedResponse<CourseDto>>({ dados: [] })),
          shareReplay({ bufferSize: 1, refCount: true })
        );

      if (Object.keys(filter).length === 0) {
        this.cache$ = request;
      }

      return request;
    }

    return this.cache$;
  }
}
