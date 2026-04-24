import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin, of } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

import { CourseDto, CourseListItem } from '../../core/api/courses.api';
import { AiFrequentQuestionItemDto } from '../../core/api/ai.api';
import { ForumPostDto, ForumThreadDto } from '../../core/api/forum.api';
import { AiInsightsService } from '../../core/services/ai-insights.service';
import { AuthService } from '../../core/services/auth.service';
import { ClassGroupsService } from '../../core/services/class-groups.service';
import { CourseSubscriptionsService } from '../../core/services/course-subscriptions.service';
import { CoursesService } from '../../core/services/courses.service';
import { ForumService } from '../../core/services/forum.service';

interface HighlightCard {
  label: string;
  value: string;
  trend: 'up' | 'down' | 'steady';
  context: string;
}

interface ForumActivityItem {
  threadId: string;
  title: string;
  courseTitle: string;
  classGroupName: string;
  lastAuthorName: string;
  lastActivityAt: Date;
}

interface StudentCourseCard extends CourseListItem {
  subscribedAt?: string;
  enrollmentType: 'distribution' | 'interactive';
  subscriptionId?: string;
  enrollmentId?: string;
  classGroupName?: string;
  enrollmentStatus?: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent {
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly coursesService = inject(CoursesService);
  private readonly subscriptionsService = inject(CourseSubscriptionsService);
  private readonly classGroupsService = inject(ClassGroupsService);
  private readonly forumService = inject(ForumService);
  private readonly aiInsightsService = inject(AiInsightsService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly toastr = inject(ToastrService);
  private readonly modeLabels: Record<number, string> = {
    1: 'Interactivo',
    2: 'Assincrono'
  };
  private readonly enrollmentStatusLabels: Record<number, string> = {
    1: 'Pendente',
    2: 'Aprovada',
    3: 'Recusada',
    4: 'Cancelada'
  };

  readonly currentUser = this.authService.currentUser;
  readonly isStudentView = computed(() => this.currentUser()?.role === 1);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly studentCourses = signal<StudentCourseCard[]>([]);
  readonly leavingCourseId = signal<string | null>(null);

  readonly highlightCards = signal<HighlightCard[]>([
    { label: 'ALUNOS MATRICULADOS', value: '0', trend: 'steady', context: 'Total nos seus cursos' },
    { label: 'CURSOS INTERATIVOS', value: '0', trend: 'steady', context: 'Criados por voce' },
    { label: 'CURSOS NÃO INTERATIVOS', value: '0', trend: 'steady', context: 'Criados por voce' },
  ]);

  readonly forumActivities = signal<ForumActivityItem[]>([]);
  readonly forumActivityLoading = signal(false);
  readonly forumActivityError = signal<string | null>(null);
  readonly frequentQuestions = signal<AiFrequentQuestionItemDto[]>([]);
  readonly frequentQuestionsLoading = signal(false);
  readonly frequentQuestionsError = signal<string | null>(null);

  constructor() {
    const userId = this.currentUser()?.id;
    if (!userId) {
      this.error.set('Usuario nao identificado.');
      this.loading.set(false);
      return;
    }

    if (this.isStudentView()) {
      this.loadStudentCourses(userId);
    } else {
      this.loading.set(false);
      this.loadInstructorStats(userId);
      this.loadInstructorForumActivity(userId);
      this.loadInstructorFrequentQuestions();
    }
  }

  goToCreateCourse(): void {
    this.router.navigate(['/courses/create']);
  }

  trackByCourse(_: number, item: StudentCourseCard): string {
    return item.id;
  }

  enrollmentStatusLabel(status?: number): string {
    return status ? this.enrollmentStatusLabels[status] ?? 'Desconhecido' : '';
  }

  trackByForumActivity(_: number, item: ForumActivityItem): string {
    return item.threadId;
  }

  trackByFrequentQuestion(_: number, item: AiFrequentQuestionItemDto): string {
    return `${item.Topic}-${item.CourseTitle}-${item.ClassGroupName}`;
  }

  enrollmentStatusClass(status?: number): string {
    switch (status) {
      case 2:
        return 'status-badge status-badge--success';
      case 1:
        return 'status-badge status-badge--warning';
      case 3:
        return 'status-badge status-badge--danger';
      case 4:
        return 'status-badge status-badge--neutral';
      default:
        return 'status-badge status-badge--neutral';
    }
  }

  canAccessCourse(course: StudentCourseCard): boolean {
    if (course.enrollmentType === 'distribution') {
      return true;
    }
    return course.enrollmentStatus === 2;
  }

  openCourse(courseId: string): void {
    this.router.navigate(['/student/courses', courseId]);
  }

  isLeavingCourse(courseId: string): boolean {
    return this.leavingCourseId() === courseId;
  }

  leaveCourse(course: StudentCourseCard): void {
    if (this.isLeavingCourse(course.id)) {
      return;
    }

    const confirmed = window.confirm(`Deseja sair do curso "${course.title}"?`);
    if (!confirmed) {
      return;
    }

    const request =
      course.enrollmentType === 'distribution' && course.subscriptionId
        ? this.subscriptionsService.remove(course.subscriptionId)
        : course.enrollmentType === 'interactive' && course.enrollmentId
          ? this.classGroupsService.removeEnrollment(course.enrollmentId)
          : null;

    if (!request) {
      this.toastr.error('Nao foi possivel identificar sua inscricao neste curso.');
      return;
    }

    this.leavingCourseId.set(course.id);
    request
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.studentCourses.update(current => current.filter(item => item.id !== course.id));
          this.leavingCourseId.set(null);
          this.toastr.success('Voce saiu do curso com sucesso.');
        },
        error: error => {
          this.leavingCourseId.set(null);
          this.toastr.error(this.extractErrorMessage(error, 'Nao foi possivel sair do curso.'));
        }
      });
  }

  private loadStudentCourses(studentId: string): void {
    forkJoin({
      subscriptions: this.subscriptionsService.getByStudent(studentId),
      courseCards: this.coursesService.getCourseCards({ IsPublished: true }),
      courseDetails: this.coursesService.getCoursesDto({ IsPublished: true })
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ subscriptions, courseCards, courseDetails }) => {
          const cardsById = new Map(courseCards.map(course => [course.id, course]));
          const courseDtoById = new Map(courseDetails.map(course => [course.Id, course]));
          const mergedCourses: StudentCourseCard[] = [];
          const addedCourseIds = new Set<string>();

          subscriptions.forEach(subscription => {
            const courseCard =
              cardsById.get(subscription.CourseId) ?? this.mapCourseDtoToListItem(courseDtoById.get(subscription.CourseId));
            if (!courseCard) {
              return;
            }
            mergedCourses.push({
              ...courseCard,
              subscribedAt: subscription.SubscribedAt,
              enrollmentType: 'distribution',
              subscriptionId: subscription.Id
            });
            addedCourseIds.add(courseCard.id);
          });

          courseDetails.forEach(course => {
            if (addedCourseIds.has(course.Id)) {
              return;
            }
            const enrollment = this.resolveStudentEnrollment(course, studentId);
            if (!enrollment) {
              return;
            }
            const courseCard = cardsById.get(course.Id) ?? this.mapCourseDtoToListItem(course);
            if (!courseCard) {
              return;
            }
            mergedCourses.push({
              ...courseCard,
              subscribedAt: enrollment.date,
              enrollmentType: 'interactive',
              enrollmentId: enrollment.id,
              classGroupName: enrollment.groupName,
              enrollmentStatus: enrollment.status
            });
            addedCourseIds.add(course.Id);
          });

          this.studentCourses.set(mergedCourses);
          this.error.set(null);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Nao foi possivel carregar seus cursos inscritos.');
          this.loading.set(false);
        }
      });
  }

  private loadInstructorStats(instructorId: string): void {
    this.coursesService
      .getCoursesDto({ InstructorId: instructorId, PageSize: 200 })
      .pipe(
        switchMap(courses => {
          const interactiveCourses = courses.filter(course => course.Mode === 1);
          const nonInteractiveCourses = courses.filter(course => course.Mode !== 1);

          const interactiveStudents = interactiveCourses.reduce((total, course) => {
            const groups = course.ClassGroups ?? [];
            const approved = groups.reduce((sum, group) => sum + (group.ApprovedEnrollments ?? 0), 0);
            const pending = groups.reduce((sum, group) => sum + (group.PendingEnrollments ?? 0), 0);
            return total + approved + pending;
          }, 0);

          const nonInteractiveIds = nonInteractiveCourses.map(course => course.Id);
          const subscriptions$ = nonInteractiveIds.length
            ? forkJoin(nonInteractiveIds.map(id => this.subscriptionsService.getByCourse(id)))
            : of([]);

          return subscriptions$.pipe(
            map(subscriptions => ({
              interactiveCount: interactiveCourses.length,
              nonInteractiveCount: nonInteractiveCourses.length,
              interactiveStudents,
              subscriptions
            }))
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: ({ interactiveCount, nonInteractiveCount, interactiveStudents, subscriptions }) => {
          const subscriptionCount = (subscriptions as Array<{ length: number }>).reduce(
            (total, list) => total + list.length,
            0
          );
          const totalStudents = interactiveStudents + subscriptionCount;
          const formatter = new Intl.NumberFormat('pt-BR');

          this.highlightCards.set([
            {
              label: 'ALUNOS MATRICULADOS',
              value: formatter.format(totalStudents),
              trend: 'steady',
              context: 'Total nos seus cursos'
            },
            {
              label: 'CURSOS INTERATIVOS',
              value: formatter.format(interactiveCount),
              trend: 'steady',
              context: 'Criados por voce'
            },
            {
              label: 'CURSOS NÃO INTERATIVOS',
              value: formatter.format(nonInteractiveCount),
              trend: 'steady',
              context: 'Criados por voce'
            }
          ]);
        },
        error: () => {
          this.highlightCards.set([
            { label: 'ALUNOS MATRICULADOS', value: '0', trend: 'steady', context: 'Total nos seus cursos' },
            { label: 'CURSOS INTERATIVOS', value: '0', trend: 'steady', context: 'Criados por voce' },
            { label: 'CURSOS NÃO INTERATIVOS', value: '0', trend: 'steady', context: 'Criados por voce' }
          ]);
        }
      });
  }

  private loadInstructorForumActivity(instructorId: string): void {
    this.forumActivityLoading.set(true);
    this.forumActivityError.set(null);

    this.coursesService
      .getCoursesDto({ InstructorId: instructorId, PageSize: 200 })
      .pipe(
        switchMap(courses => {
          if (!courses.length) {
            return of({ courses: [], threads: [] as ForumThreadDto[] });
          }
          const requests = courses.map(course => this.forumService.getThreadsRaw({ CourseId: course.Id, PageSize: 50 }));
          return forkJoin(requests).pipe(
            map(threadsLists => ({ courses, threads: threadsLists.flat() }))
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: ({ courses, threads }) => {
          const courseLookup = new Map(courses.map(course => [course.Id, course.Title]));
          const items = threads
            .map(thread => this.mapThreadToActivity(thread, courseLookup))
            .filter((item): item is ForumActivityItem => item !== null)
            .sort((a, b) => b.lastActivityAt.getTime() - a.lastActivityAt.getTime())
            .slice(0, 3);

          this.forumActivities.set(items);
          this.forumActivityLoading.set(false);
        },
        error: () => {
          this.forumActivityError.set('Nao foi possivel carregar as atividades do forum.');
          this.forumActivities.set([]);
          this.forumActivityLoading.set(false);
        }
      });
  }

  private loadInstructorFrequentQuestions(): void {
    this.frequentQuestionsLoading.set(true);
    this.frequentQuestionsError.set(null);

    this.aiInsightsService
      .getInstructorFrequentQuestions()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: response => {
          this.frequentQuestions.set(response.Items ?? []);
          this.frequentQuestionsLoading.set(false);
        },
        error: error => {
          this.frequentQuestions.set([]);
          this.frequentQuestionsError.set(
            this.extractErrorMessage(error, 'Nao foi possivel carregar as duvidas frequentes com IA.')
          );
          this.frequentQuestionsLoading.set(false);
        }
      });
  }

  private mapThreadToActivity(
    thread: ForumThreadDto,
    courseLookup: Map<string, string>
  ): ForumActivityItem | null {
    const lastInteraction = this.resolveLastInteraction(thread);
    if (!lastInteraction) {
      return null;
    }

    return {
      threadId: thread.Id,
      title: thread.Title,
      courseTitle: courseLookup.get(thread.CourseId) ?? 'Curso desconhecido',
      classGroupName: thread.ClassGroupName || 'Turma nao informada',
      lastAuthorName: lastInteraction.authorName,
      lastActivityAt: lastInteraction.date
    };
  }

  private resolveLastInteraction(thread: ForumThreadDto): { authorName: string; date: Date } | null {
    const posts = this.flattenPosts(thread.Posts ?? []);
    if (posts.length) {
      const latest = posts.reduce((latestPost, current) => {
        const latestDate = new Date(latestPost.CreatedAt);
        const currentDate = new Date(current.CreatedAt);
        return currentDate > latestDate ? current : latestPost;
      });

      return {
        authorName: latest.AuthorName || 'Usuario desconhecido',
        date: new Date(latest.CreatedAt)
      };
    }

    if (!thread.LastActivityAt) {
      return null;
    }

    return {
      authorName: thread.CreatedByName || 'Usuario desconhecido',
      date: new Date(thread.LastActivityAt)
    };
  }

  private flattenPosts(posts: ForumPostDto[]): ForumPostDto[] {
    const result: ForumPostDto[] = [];
    posts.forEach(post => {
      result.push(post);
      if (post.Replies?.length) {
        result.push(...this.flattenPosts(post.Replies));
      }
    });
    return result;
  }

  private mapCourseDtoToListItem(course?: CourseDto): CourseListItem | null {
    if (!course) {
      return null;
    }

    const classGroups = course.ClassGroups ?? [];
    const approvedStudents = classGroups.reduce((total, group) => total + (group.ApprovedEnrollments ?? 0), 0);
    const pendingStudents = classGroups.reduce((total, group) => total + (group.PendingEnrollments ?? 0), 0);
    const enrolledStudents = approvedStudents + pendingStudents;
    const capacity = classGroups.reduce((total, group) => total + (group.Capacity ?? 0), 0);

    return {
      id: course.Id,
      title: course.Title,
      mode: course.Mode,
      modeLabel: this.modeLabels[course.Mode] ?? 'Assincrono',
      category: course.CategoryName,
      instructor: course.InstructorName,
      published: course.IsPublished,
      publishedAt: course.PublishedAt,
      enrolledStudents,
      approvedStudents,
      pendingStudents,
      capacity,
      classGroups: classGroups.length
    };
  }

  private resolveStudentEnrollment(course: CourseDto, studentId: string) {
    const classGroups = course.ClassGroups ?? [];
    const enrollments = classGroups.flatMap(group => {
      const scopedEnrollments = group.Enrollments ?? [];
      return scopedEnrollments
        .filter(enrollment => enrollment.StudentId === studentId && [1, 2].includes(enrollment.Status))
        .map(enrollment => ({
          id: enrollment.Id,
          groupName: group.Name,
          status: enrollment.Status,
          date: enrollment.DecisionAt ?? enrollment.RequestedAt ?? new Date().toISOString()
        }));
    });

    if (!enrollments.length) {
      return null;
    }

    enrollments.sort((a, b) => {
      const priorityDiff = this.enrollmentPriority(b.status) - this.enrollmentPriority(a.status);
      if (priorityDiff !== 0) {
        return priorityDiff;
      }
      return new Date(b.date).getTime() - new Date(a.date).getTime();
    });

    return enrollments[0];
  }

  private enrollmentPriority(status: number): number {
    switch (status) {
      case 2:
        return 2;
      case 1:
        return 1;
      default:
        return 0;
    }
  }

  private extractErrorMessage(error: unknown, fallback: string): string {
    if (!(error instanceof HttpErrorResponse)) {
      return fallback;
    }

    const payload = error.error;
    if (typeof payload === 'string' && payload.trim()) {
      return payload;
    }

    if (payload && typeof payload === 'object') {
      const message = (payload.message ?? payload.Message ?? payload.title ?? payload.Title) as string | undefined;
      if (message?.trim()) {
        return message;
      }
    }

    return fallback;
  }
}
