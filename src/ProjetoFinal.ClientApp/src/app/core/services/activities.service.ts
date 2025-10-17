import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, of } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ActivityDto, ActivityListItem, mapActivitiesResponse } from '../api/activities.api';
import { ApiPagedResponse } from '../api/api.types';
import { toHttpParams } from '../utils/http-params.util';

@Injectable({ providedIn: 'root' })
export class ActivitiesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.baseUrl;

  getActivities() {
    const params = toHttpParams({ PageSize: 50 });
    return this.http
      .get<ApiPagedResponse<ActivityDto>>(`${this.baseUrl}/activities`, { params })
      .pipe(
        catchError(() => of<ApiPagedResponse<ActivityDto>>({ dados: [] })),
        map(mapActivitiesResponse)
      );
  }
}
