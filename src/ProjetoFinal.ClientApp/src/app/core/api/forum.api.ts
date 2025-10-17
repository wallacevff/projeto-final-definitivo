import { ApiPagedResponse, normalizePagedResponse } from './api.types';
import { formatRelativeHours } from '../utils/date.util';

export interface ForumThreadDto {
  Id: string;
  CourseId: string;
  ClassGroupId: string;
  ClassGroupName: string;
  CreatedById: string;
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
  isPinned: boolean;
  isLocked: boolean;
}

export function mapForumThreadsResponse(response: ApiPagedResponse<ForumThreadDto>, courseLookup: Map<string, string>): ForumThreadListItem[] {
  const { items } = normalizePagedResponse(response);

  return items.map(thread => ({
    id: thread.Id,
    title: thread.Title,
    courseTitle: courseLookup.get(thread.CourseId) ?? 'Curso desconhecido',
    classGroupName: thread.ClassGroupName || 'Turma nao informada',
    replies: countReplies(thread.Posts ?? []),
    lastActivityLabel: formatRelativeHours(thread.LastActivityAt),
    authorId: thread.CreatedById,
    isPinned: thread.IsPinned,
    isLocked: thread.IsLocked
  } satisfies ForumThreadListItem));
}

function countReplies(posts: ForumPostDto[]): number {
  let total = posts.length;
  posts.forEach(post => {
    total += countReplies(post.Replies ?? []);
  });
  return total;
}
