import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, of, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  ActivityCreatePayload,
  ActivityDto,
  ActivityListItem,
  ActivitiesFilter,
  mapActivitiesResponse
} from '../api/activities.api';
import { ApiPagedResponse } from '../api/api.types';
import { toHttpParams } from '../utils/http-params.util';

@Injectable({ providedIn: 'root' })
export class ActivitiesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.baseUrl;

  getActivities(filter: ActivitiesFilter = {}) {
    const params = toHttpParams({ PageSize: 50, ...filter });
    return this.http
      .get<ApiPagedResponse<ActivityDto>>(`${this.baseUrl}/activities`, { params })
      .pipe(
        catchError(() => of<ApiPagedResponse<ActivityDto>>({ dados: [] })),
        map(mapActivitiesResponse)
      );
  }

  getActivityById(activityId: string) {
    return this.http
      .get<ActivityDto>(`${this.baseUrl}/activities/${activityId}`)
      .pipe(catchError(error => throwError(() => error)));
  }

  createActivity(payload: ActivityCreatePayload) {
    return this.http
      .post<ActivityDto>(`${this.baseUrl}/activities`, payload)
      .pipe(catchError(error => throwError(() => error)));
  }
}
