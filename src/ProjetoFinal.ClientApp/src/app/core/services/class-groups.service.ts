import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, forkJoin, Observable, of, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ClassGroupCreatePayload, ClassGroupDto } from '../api/courses.api';

@Injectable({ providedIn: 'root' })
export class ClassGroupsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.baseUrl;

  getClassGroupById(classGroupId: string): Observable<ClassGroupDto> {
    return this.http
      .get<ClassGroupDto>(`${this.baseUrl}/class-groups/${classGroupId}`)
      .pipe(catchError(error => throwError(() => error)));
  }

  createClassGroup(payload: ClassGroupCreatePayload): Observable<ClassGroupDto> {
    return this.http
      .post<ClassGroupDto>(`${this.baseUrl}/class-groups`, payload)
      .pipe(catchError(error => throwError(() => error)));
  }

  createMany(payloads: ClassGroupCreatePayload[]): Observable<ClassGroupDto[]> {
    if (!payloads.length) {
      return of([]);
    }

    return forkJoin(payloads.map(payload => this.createClassGroup(payload)));
  }
}
