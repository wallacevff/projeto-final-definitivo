import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, of, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  ActivitySubmissionCreatePayload,
  ActivitySubmissionDto,
  ActivitySubmissionFilter,
  mapSubmissionsResponse
} from '../api/activity-submissions.api';
import { ApiPagedResponse } from '../api/api.types';
import { toHttpParams } from '../utils/http-params.util';

@Injectable({ providedIn: 'root' })
export class ActivitySubmissionsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.baseUrl;

  submit(payload: ActivitySubmissionCreatePayload) {
    return this.http
      .post<ActivitySubmissionDto>(`${this.baseUrl}/activity-submissions`, payload)
      .pipe(catchError(error => throwError(() => error)));
  }

  getById(submissionId: string) {
    return this.http
      .get<ActivitySubmissionDto>(`${this.baseUrl}/activity-submissions/${submissionId}`)
      .pipe(catchError(error => throwError(() => error)));
  }

  getSubmissions(filter: ActivitySubmissionFilter = {}) {
    const params = toHttpParams({ PageSize: 20, PageNumber: 1, ...filter });

    return this.http
      .get<ApiPagedResponse<ActivitySubmissionDto>>(`${this.baseUrl}/activity-submissions`, { params })
      .pipe(
        catchError(() => of<ApiPagedResponse<ActivitySubmissionDto>>({ dados: [] })),
        map(mapSubmissionsResponse)
      );
  }

  getStudentSubmission(activityId: string, studentId: string) {
    return this.getSubmissions({ ActivityId: activityId, StudentId: studentId, PageSize: 1 }).pipe(
      map(response => response.items[0] ?? null)
    );
  }
}
