import { ApiPagedResponse, normalizePagedResponse } from './api.types';
import { formatRelativeHours } from '../utils/date.util';

export interface ForumThreadDto {
  Id: string;
  CourseId: string;
  ClassGroupId: string;
  ClassGroupName: string;
  CreatedById: string;
  CreatedByName: string;
  Title: string;
  Description?: string;
  IsLocked: boolean;
  IsPinned: boolean;
  LastActivityAt: string;
  Posts: ForumPostDto[];
}

export interface ForumPostDto {
  Id: string;
  ThreadId: string;
  AuthorId: string;
  AuthorName: string;
  ParentPostId?: string;
  Message: string;
  CreatedAt: string;
  EditedAt?: string;
  Replies: ForumPostDto[];
}

export interface ForumThreadListItem {
  id: string;
  title: string;
  courseTitle: string;
  classGroupName: string;
  replies: number;
  lastActivityLabel: string;
  authorId: string;
  authorName: string;
  isPinned: boolean;
  isLocked: boolean;
}

export interface ForumThreadFilter {
  CourseId?: string;
  ClassGroupId?: string;
  Title?: string;
  IsPinned?: boolean;
  PageNumber?: number;
  PageSize?: number;
}

export interface ForumThreadCreatePayload {
  ClassGroupId: string;
  CreatedById: string;
  Title: string;
  Description?: string;
  IsPinned: boolean;
}

export interface ForumPostFilter {
  ThreadId?: string;
  AuthorId?: string;
  ParentPostId?: string;
  PageNumber?: number;
  PageSize?: number;
}

export interface ForumPostCreatePayload {
  ThreadId: string;
  AuthorId: string;
  ParentPostId?: string;
  Message: string;
}

export function mapForumThreadsResponse(
  response: ApiPagedResponse<ForumThreadDto>,
  courseLookup: Map<string, string>
): ForumThreadListItem[] {
  const { items } = normalizePagedResponse(response);

  return items.map(thread => ({
    id: thread.Id,
    title: thread.Title,
    courseTitle: courseLookup.get(thread.CourseId) ?? 'Curso desconhecido',
    classGroupName: thread.ClassGroupName || 'Turma nao informada',
    replies: countReplies(thread.Posts ?? []),
    lastActivityLabel: formatRelativeHours(thread.LastActivityAt),
    authorId: thread.CreatedById,
    authorName: thread.CreatedByName || 'Usuario desconhecido',
    isPinned: thread.IsPinned,
    isLocked: thread.IsLocked
  }));
}

function countReplies(posts: ForumPostDto[]): number {
  let total = posts.length;
  posts.forEach(post => {
    total += countReplies(post.Replies ?? []);
  });
  return total;
}
