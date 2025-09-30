import { ApiPagedResponse, normalizePagedResponse } from './api.types';

export interface CourseDto {
  Id: string;
  Title: string;
  Slug: string;
  ShortDescription: string;
  DetailedDescription?: string;
  Mode: number;
  CategoryId: string;
  CategoryName: string;
  InstructorId: string;
  InstructorName: string;
  ThumbnailMediaId?: string;
  EnableForum: boolean;
  EnableChat: boolean;
  IsPublished: boolean;
  CreatedAt: string;
  PublishedAt?: string;
  EnrollmentInstructions?: string;
  ClassGroups: ClassGroupDto[];
}

export interface ClassGroupDto {
  Id: string;
  CourseId: string;
  Name: string;
  Description?: string;
  Capacity: number;
  RequiresApproval: boolean;
  RequiresEnrollmentCode: boolean;
  EnableChat: boolean;
  EnrollmentOpensAt?: string;
  EnrollmentClosesAt?: string;
  StartsAt?: string;
  EndsAt?: string;
  ApprovedEnrollments: number;
  PendingEnrollments: number;
}

export interface CoursesFilter {
  PageSize?: number;
  PageNumber?: number;
  Title?: string;
  CategoryId?: string;
  InstructorId?: string;
  IsPublished?: boolean;
}

export interface CourseListItem {
  id: string;
  title: string;
  modeLabel: string;
  category: string;
  instructor: string;
  published: boolean;
  publishedAt?: string;
  enrolledStudents: number;
  capacity: number;
  classGroups: number;
}

const courseModeLabel: Record<number, string> = {
  1: 'Interactivo',
  2: 'Assincrono'
};

export function mapCoursesResponse(response: ApiPagedResponse<CourseDto>): CourseListItem[] {
  const { items } = normalizePagedResponse(response);
  return items.map(mapCourseDtoToListItem);
}

function mapCourseDtoToListItem(course: CourseDto): CourseListItem {
  const classGroups = course.ClassGroups ?? [];
  const enrolledStudents = classGroups.reduce((total, group) => total + (group.ApprovedEnrollments ?? 0), 0);
  const capacity = classGroups.reduce((total, group) => total + (group.Capacity ?? 0), 0);

  return {
    id: course.Id,
    title: course.Title,
    modeLabel: courseModeLabel[course.Mode] ?? 'Assincrono',
    category: course.CategoryName,
    instructor: course.InstructorName,
    published: course.IsPublished,
    publishedAt: course.PublishedAt,
    enrolledStudents,
    capacity,
    classGroups: classGroups.length
  };
}

export interface ClassGroupListItem {
  id: string;
  courseId: string;
  courseTitle: string;
  name: string;
  capacity: number;
  occupied: number;
  requiresApproval: boolean;
  status: 'Concluida' | 'Em andamento' | 'Inscricoes abertas';
  nextEvent: string;
}

export function mapClassGroupsFromCourses(courses: CourseDto[], now = new Date()): ClassGroupListItem[] {
  const formatter = new Intl.DateTimeFormat('pt-BR', {
    day: '2-digit',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit'
  });

  return courses.flatMap(course => {
    const groups = course.ClassGroups ?? [];
    return groups.map(group => {
      const occupied = group.ApprovedEnrollments ?? 0;
      const capacity = group.Capacity ?? 0;
      const status = resolveClassGroupStatus(group, now);
      const nextEvent = formatNextEvent(group, formatter);

      return {
        id: group.Id,
        courseId: course.Id,
        courseTitle: course.Title,
        name: group.Name,
        capacity,
        occupied,
        requiresApproval: group.RequiresApproval,
        status,
        nextEvent
      } satisfies ClassGroupListItem;
    });
  });
}

function resolveClassGroupStatus(group: ClassGroupDto, now: Date): ClassGroupListItem['status'] {
  const endsAt = group.EndsAt ? new Date(group.EndsAt) : undefined;
  const startsAt = group.StartsAt ? new Date(group.StartsAt) : undefined;

  if (endsAt && !Number.isNaN(endsAt.getTime()) && endsAt.getTime() < now.getTime()) {
    return 'Concluida';
  }

  if (startsAt && !Number.isNaN(startsAt.getTime()) && startsAt.getTime() <= now.getTime()) {
    return 'Em andamento';
  }

  return 'Inscricoes abertas';
}

function formatNextEvent(group: ClassGroupDto, formatter: Intl.DateTimeFormat): string {
  const startsAt = group.StartsAt ? new Date(group.StartsAt) : null;
  const endsAt = group.EndsAt ? new Date(group.EndsAt) : null;

  if (startsAt && !Number.isNaN(startsAt.getTime()) && startsAt.getTime() > Date.now()) {
    return `Inicio · ${formatter.format(startsAt).replace('.', '')}`;
  }

  if (endsAt && !Number.isNaN(endsAt.getTime())) {
    return `Fim previsto · ${formatter.format(endsAt).replace('.', '')}`;
  }

  return 'Agenda nao definida';
}
