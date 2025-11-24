import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, Observable, of, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiPagedResponse, normalizePagedResponse } from '../api/api.types';
import { UserDto, UserRole } from '../api/users.api';
import { toHttpParams } from '../utils/http-params.util';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.baseUrl;

  getInstructors(): Observable<UserDto[]> {
    const params = toHttpParams({ Role: UserRole.Instructor, PageSize: 100 });
    return this.http
      .get<ApiPagedResponse<UserDto>>(`${this.baseUrl}/users`, { params })
      .pipe(
        map(response => normalizePagedResponse(response).items),
        catchError(() => of<UserDto[]>([]))
      );
  }

  getById(userId: string): Observable<UserDto> {
    return this.http
      .get<UserDto>(`${this.baseUrl}/users/${userId}`)
      .pipe(catchError(error => throwError(() => error)));
  }
}
