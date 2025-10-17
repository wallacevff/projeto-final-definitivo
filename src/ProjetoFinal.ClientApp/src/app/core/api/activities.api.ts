import { ApiPagedResponse, normalizePagedResponse } from './api.types';
import { formatDateLabel } from '../utils/date.util';

export interface ActivityDto {
  Id: string;
  CourseId: string;
  ClassGroupId: string;
  ClassGroupName: string;
  ReferenceContentId?: string;
  Scope: number;
  Title: string;
  Description: string;
  AvailableAt?: string;
  DueDate?: string;
  MaxScore?: number;
  AllowLateSubmissions: boolean;
  LatePenaltyPercentage?: number;
  VisibleToStudents: boolean;
  CreatedById: string;
  Audiences: ActivityAudienceDto[];
  Attachments: ActivityAttachmentDto[];
}

export interface ActivityAudienceDto {
  ClassGroupId: string;
  ClassGroupName: string;
}

export interface ActivityAttachmentDto {
  Id: string;
  MediaResourceId: string;
  Caption?: string;
}

export interface ActivityListItem {
  id: string;
  title: string;
  dueDateLabel: string;
  courseId: string;
  classGroupId: string;
  classGroupName: string;
  allowLate: boolean;
  attachments: number;
}

export function mapActivitiesResponse(response: ApiPagedResponse<ActivityDto>): ActivityListItem[] {
  const { items } = normalizePagedResponse(response);

  return items.map(item => ({
    id: item.Id,
    title: item.Title,
    dueDateLabel: formatDateLabel(item.DueDate, { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' }),
    courseId: item.CourseId,
    classGroupId: item.ClassGroupId,
    classGroupName: item.ClassGroupName || inferClassGroupName(item),
    allowLate: item.AllowLateSubmissions,
    attachments: item.Attachments?.length ?? 0
  } satisfies ActivityListItem));
}

function inferClassGroupName(activity: ActivityDto): string {
  const [firstAudience] = activity.Audiences ?? [];
  return firstAudience?.ClassGroupName || 'Turma nao informada';
}
