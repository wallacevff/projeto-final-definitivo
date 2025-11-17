import { ApiPagedResponse, normalizePagedResponse } from './api.types';
import { MediaResource } from './media.api';

export interface ActivitySubmissionDto {
  Id: string;
  ActivityId: string;
  StudentId: string;
  ClassGroupId?: string;
  Status: number;
  SubmittedAt: string;
  GradedAt?: string;
  GradedById?: string;
  Score?: number;
  Feedback?: string;
  TextAnswer?: string;
  Attachments: SubmissionAttachmentDto[];
}

export interface SubmissionAttachmentDto {
  Id: string;
  MediaResourceId: string;
  IsPrimary: boolean;
  IsVideo: boolean;
  Media?: MediaResource;
}

export interface ActivitySubmissionCreatePayload {
  ActivityId: string;
  StudentId: string;
  ClassGroupId?: string;
  TextAnswer?: string;
  Attachments: SubmissionAttachmentCreatePayload[];
}

export interface SubmissionAttachmentCreatePayload {
  MediaResourceId: string;
  IsPrimary: boolean;
  IsVideo: boolean;
}

export interface ActivitySubmissionFilter {
  ActivityId?: string;
  StudentId?: string;
  ClassGroupId?: string;
  Status?: number;
  PageNumber?: number;
  PageSize?: number;
}

export function mapSubmissionsResponse(response: ApiPagedResponse<ActivitySubmissionDto>) {
  return normalizePagedResponse(response);
}
