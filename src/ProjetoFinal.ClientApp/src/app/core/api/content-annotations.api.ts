import { ApiPagedResponse, normalizePagedResponse } from './api.types';

export interface ContentVideoAnnotationDto {
  Id: string;
  ContentAttachmentId: string;
  CreatedById: string;
  TimeMarkerSeconds: number;
  Comment: string;
  EditedAt?: string;
}

export interface ContentVideoAnnotationCreatePayload {
  ContentAttachmentId: string;
  TimeMarkerSeconds: number;
  Comment: string;
}

export interface ContentVideoAnnotationFilter {
  ContentAttachmentId?: string;
}

export function mapContentAnnotationsResponse(response: ApiPagedResponse<ContentVideoAnnotationDto>) {
  return normalizePagedResponse(response);
}
