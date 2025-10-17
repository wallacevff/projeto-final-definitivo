import { ApiPagedResponse, normalizePagedResponse } from './api.types';

export interface CourseCategoryDto {
  Id: string;
  Name: string;
  Description?: string;
  IsPublished: boolean;
}

export function extractCourseCategories(response: ApiPagedResponse<CourseCategoryDto>): CourseCategoryDto[] {
  return normalizePagedResponse(response).items;
}
