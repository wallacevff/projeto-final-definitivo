import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, of, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { CourseSubscriptionCreatePayload, CourseSubscriptionDto } from '../api/course-subscriptions.api';
import { ApiPagedResponse, normalizePagedResponse } from '../api/api.types';
import { toHttpParams } from '../utils/http-params.util';

@Injectable({ providedIn: 'root' })
export class CourseSubscriptionsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.baseUrl;

  getByStudent(studentId: string) {
    const params = toHttpParams({ StudentId: studentId, PageSize: 100 });
    return this.http
      .get<ApiPagedResponse<CourseSubscriptionDto>>(`${this.baseUrl}/course-subscriptions`, { params })
      .pipe(
        map(response => normalizePagedResponse(response).items),
        catchError(() => of<CourseSubscriptionDto[]>([]))
      );
  }

  getByCourse(courseId: string) {
    const params = toHttpParams({ CourseId: courseId, PageSize: 500 });
    return this.http
      .get<ApiPagedResponse<CourseSubscriptionDto>>(`${this.baseUrl}/course-subscriptions`, { params })
      .pipe(
        map(response => normalizePagedResponse(response).items),
        catchError(() => of<CourseSubscriptionDto[]>([]))
      );
  }

  subscribe(payload: CourseSubscriptionCreatePayload) {
    return this.http
      .post<CourseSubscriptionDto>(`${this.baseUrl}/course-subscriptions`, payload)
      .pipe(catchError(error => throwError(() => error)));
  }
}
