import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin, of, EMPTY } from 'rxjs';
import { distinctUntilChanged, filter, map, switchMap, tap } from 'rxjs/operators';

import { CoursesService } from '../../core/services/courses.service';
import { ActivitiesService } from '../../core/services/activities.service';
import { CourseContentsService } from '../../core/services/course-contents.service';
import { ForumService } from '../../core/services/forum.service';
import { AuthService } from '../../core/services/auth.service';
import { CourseSubscriptionsService } from '../../core/services/course-subscriptions.service';
import { ActivitySubmissionsService } from '../../core/services/activity-submissions.service';
import { CourseDto } from '../../core/api/courses.api';
import { ActivityListItem } from '../../core/api/activities.api';
import { CourseContentListItem } from '../../core/api/contents.api';
import { ForumThreadListItem } from '../../core/api/forum.api';

type CourseSection = 'activities' | 'contents' | 'forum';

@Component({
  selector: 'app-student-course-view',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './student-course-view.component.html',
  styleUrl: './student-course-view.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StudentCourseViewComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly authService = inject(AuthService);
  private readonly coursesService = inject(CoursesService);
  private readonly activitiesService = inject(ActivitiesService);
  private readonly contentsService = inject(CourseContentsService);
  private readonly forumService = inject(ForumService);
  private readonly subscriptionsService = inject(CourseSubscriptionsService);
  private readonly submissionsService = inject(ActivitySubmissionsService);

  readonly currentUser = this.authService.currentUser;
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly course = signal<CourseDto | null>(null);
  readonly activities = signal<ActivityListItem[]>([]);
  readonly completedActivityIds = signal<Set<string>>(new Set());
  readonly activityStatusFilter = signal<'all' | 'pending' | 'completed'>('all');
  readonly contents = signal<CourseContentListItem[]>([]);
  readonly forumThreads = signal<ForumThreadListItem[]>([]);
  readonly sections: { key: CourseSection; label: string; description: string }[] = [
    {
      key: 'activities',
      label: 'Atividades',
      description: 'Prazos, entregas e avaliacoes publicadas para este curso.'
    },
    {
      key: 'contents',
      label: 'Conteudos',
      description: 'Materiais publicados pelo instrutor, em video, texto ou anexos.'
    },
    {
      key: 'forum',
      label: 'Forum',
      description: 'Discussoes e anuncios compartilhados com a turma.'
    }
  ];
  readonly activeSection = signal<CourseSection>('activities');
  readonly filteredActivities = signal<ActivityListItem[]>([]);

  constructor() {
    this.route.paramMap
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        map(params => params.get('courseId')),
        filter((courseId): courseId is string => Boolean(courseId)),
        distinctUntilChanged(),
        switchMap(courseId => {
          const studentId = this.currentUser()?.id;
          if (!studentId) {
            this.loading.set(false);
            this.error.set('Usuario nao identificado.');
            return EMPTY;
          }

          this.loading.set(true);
          this.error.set(null);
          this.activeSection.set('activities');
          this.activities.set([]);
          this.completedActivityIds.set(new Set());
          this.activityStatusFilter.set('all');
          this.contents.set([]);
          this.forumThreads.set([]);

          return this.coursesService.getCourseById(courseId).pipe(
            tap(course => this.course.set(course)),
            switchMap(course => {
              return this.subscriptionsService.getByStudent(studentId).pipe(
                map(subscriptions => subscriptions.some(subscription => subscription.CourseId === course.Id)),
                switchMap(isSubscribed => {
                  const studentGroupIds = this.resolveAccessibleClassGroups(course, studentId, isSubscribed);
                  const courseLookup = new Map<string, string>([[course.Id, course.Title]]);

                  const forum$ = course.EnableForum
                    ? this.forumService.getThreads(courseLookup, { CourseId: course.Id })
                    : of<ForumThreadListItem[]>([]);
                  const activities$ = this.loadActivitiesForStudent(course, studentGroupIds);
                  const submissions$ = this.loadSubmissionsForStudent(studentId, studentGroupIds);

                  return forkJoin({
                    activities: activities$,
                    contents: this.contentsService.getContents({ CourseId: course.Id }),
                    forumThreads: forum$,
                    submissions: submissions$
                  });
                })
              );
            })
          );
        })
      )
      .subscribe({
        next: ({ activities, contents, forumThreads, submissions }) => {
          const completedIds = new Set(
            submissions
              .filter(submission => submission.Status >= 2)
              .map(submission => submission.ActivityId)
          );
          this.activities.set(activities);
          this.completedActivityIds.set(completedIds);
          this.applyActivityFilter();
          this.contents.set(contents);
          this.forumThreads.set(forumThreads);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Nao foi possivel carregar os dados do curso.');
          this.loading.set(false);
        }
      });
  }

  setSection(section: CourseSection): void {
    this.activeSection.set(section);
  }

  setActivityStatusFilter(filter: 'all' | 'pending' | 'completed'): void {
    this.activityStatusFilter.set(filter);
    this.applyActivityFilter();
  }

  backToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }

  openForum(): void {
    this.router.navigate(['/forum']);
  }

  openForumThread(threadId: string): void {
    if (!threadId) {
      return;
    }
    this.router.navigate(['/forum/threads', threadId]);
  }

  private resolveAccessibleClassGroups(course: CourseDto, studentId: string, isSubscribed: boolean): string[] {
    const groups = course.ClassGroups ?? [];
    if (!groups.length) {
      return [];
    }

    const allowedStatus = new Set([1, 2]);
    const allowedGroups = new Set<string>();

    groups.forEach(group => {
      if (group.IsMaterialsDistribution) {
        if (isSubscribed) {
          allowedGroups.add(group.Id);
        }
        return;
      }

      const enrollments = group.Enrollments ?? [];
      const hasEnrollment = enrollments.some(
        enrollment => enrollment.StudentId === studentId && allowedStatus.has(enrollment.Status)
      );
      if (hasEnrollment) {
        allowedGroups.add(group.Id);
      }
    });

    return Array.from(allowedGroups);
  }

  private loadActivitiesForStudent(course: CourseDto, classGroupIds: string[]) {
    if (classGroupIds.length === 0) {
      return of<ActivityListItem[]>([]);
    }

    if (classGroupIds.length === 1) {
      return this.activitiesService.getActivities({
        CourseId: course.Id,
        ClassGroupId: classGroupIds[0],
        VisibleToStudents: true
      });
    }

    const requests = classGroupIds.map(classGroupId =>
      this.activitiesService.getActivities({ CourseId: course.Id, ClassGroupId: classGroupId, VisibleToStudents: true })
    );
    return forkJoin(requests).pipe(map(results => results.flat()));
  }

  private loadSubmissionsForStudent(studentId: string, classGroupIds: string[]) {
    if (classGroupIds.length === 0) {
      return of([]);
    }

    const requests = classGroupIds.map(classGroupId =>
      this.submissionsService.getSubmissions({ StudentId: studentId, ClassGroupId: classGroupId, PageSize: 200 })
    );

    return forkJoin(requests).pipe(map(results => results.flatMap(result => result.items)));
  }

  private applyActivityFilter(): void {
    const filter = this.activityStatusFilter();
    const completedIds = this.completedActivityIds();
    const items = this.activities();

    if (filter === 'all') {
      this.filteredActivities.set(items);
      return;
    }

    const isCompleted = (activityId: string) => completedIds.has(activityId);
    const filtered = items.filter(activity =>
      filter === 'completed' ? isCompleted(activity.id) : !isCompleted(activity.id)
    );
    this.filteredActivities.set(filtered);
  }
}
