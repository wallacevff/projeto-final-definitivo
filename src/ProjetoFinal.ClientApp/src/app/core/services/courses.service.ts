import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, of, shareReplay, Observable, throwError, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { CourseDto, CoursesFilter, mapClassGroupsFromCourses, mapCoursesResponse } from '../api/courses.api';
import { ApiPagedResponse, normalizePagedResponse } from '../api/api.types';
import { toHttpParams } from '../utils/http-params.util';

export interface CreateCoursePayload {
  Title: string;
  ShortDescription: string;
  DetailedDescription?: string;
  Mode: number;
  CategoryName: string;
  EnableForum: boolean;
  EnableChat: boolean;
  EnrollmentInstructions?: string;
  ThumbnailMediaId?: string;
}

export interface UpdateCoursePayload {
  Title: string;
  ShortDescription: string;
  DetailedDescription?: string;
  Mode: number;
  CategoryName: string;
  EnableForum: boolean;
  EnableChat: boolean;
  IsPublished: boolean;
  EnrollmentInstructions?: string;
  ThumbnailMediaId?: string;
}

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

  getCourseById(courseId: string) {
    return this.http.get<CourseDto>(`${this.baseUrl}/courses/${courseId}`).pipe(
      catchError(error => throwError(() => error))
    );
  }

  createCourse(payload: CreateCoursePayload) {
    return this.http.post<CourseDto>(`${this.baseUrl}/courses`, payload).pipe(
      tap(() => this.invalidateCache()),
      catchError(error => throwError(() => error))
    );
  }

  updateCourse(courseId: string, payload: UpdateCoursePayload) {
    return this.http.put<void>(`${this.baseUrl}/courses/${courseId}`, payload).pipe(
      tap(() => this.invalidateCache()),
      catchError(error => throwError(() => error))
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

  private invalidateCache(): void {
    this.cache$ = undefined;
  }
}
