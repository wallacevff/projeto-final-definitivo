import { ApiPagedResponse, normalizePagedResponse } from './api.types';

export enum ContentItemType {
  Text = 1,
  File = 2,
  Video = 3,
  Link = 4
}

export interface CourseContentDto {
  Id: string;
  CourseId: string;
  ClassGroupId?: string;
  AuthorId: string;
  Title: string;
  Summary?: string;
  Body?: string;
  ItemType: ContentItemType;
  IsDraft: boolean;
  DisplayOrder: number;
  PublishedAt?: string;
  Attachments: ContentAttachmentDto[];
}

export interface ContentAttachmentDto {
  Id: string;
  CourseContentId: string;
  MediaResourceId: string;
  Caption?: string;
  IsPrimary: boolean;
}

export interface ContentAttachmentPayload {
  MediaResourceId: string;
  Caption?: string;
  IsPrimary: boolean;
}

export interface CourseContentCreatePayload {
  CourseId: string;
  ClassGroupId?: string;
  AuthorId: string;
  Title: string;
  Summary?: string;
  Body?: string;
  ItemType: ContentItemType;
  IsDraft: boolean;
  DisplayOrder: number;
  Attachments: ContentAttachmentPayload[];
}

export interface CourseContentsFilter {
  CourseId?: string;
  ClassGroupId?: string;
  PageSize?: number;
  PageNumber?: number;
}

export interface CourseContentListItem {
  id: string;
  title: string;
  status: 'Rascunho' | 'Publicado';
  publishedAt?: string;
  displayOrder: number;
  attachments: number;
  itemType: ContentItemType;
}

export function mapCourseContentsResponse(response: ApiPagedResponse<CourseContentDto>): CourseContentListItem[] {
  const { items } = normalizePagedResponse(response);
  return items
    .sort((a, b) => a.DisplayOrder - b.DisplayOrder || a.Title.localeCompare(b.Title))
    .map(item => ({
      id: item.Id,
      title: item.Title,
      status: item.IsDraft ? 'Rascunho' : 'Publicado',
      publishedAt: item.PublishedAt,
      displayOrder: item.DisplayOrder,
      attachments: item.Attachments?.length ?? 0,
      itemType: item.ItemType
    }));
}
