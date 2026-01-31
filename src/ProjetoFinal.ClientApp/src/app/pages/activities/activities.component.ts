import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';

import { ActivitiesService } from '../../core/services/activities.service';
import { ActivityListItem } from '../../core/api/activities.api';
import { CoursesService } from '../../core/services/courses.service';
import { CourseDto } from '../../core/api/courses.api';
import { AuthService } from '../../core/services/auth.service';
import { CourseSubscriptionsService } from '../../core/services/course-subscriptions.service';
import { ActivitySubmissionsService } from '../../core/services/activity-submissions.service';

@Component({
  selector: 'app-activities',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './activities.component.html',
  styleUrl: './activities.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ActivitiesComponent {
  private readonly service = inject(ActivitiesService);
  private readonly coursesService = inject(CoursesService);
  private readonly subscriptionsService = inject(CourseSubscriptionsService);
  private readonly submissionsService = inject(ActivitySubmissionsService);
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly activities = signal<ActivityListItem[]>([]);
  readonly completedActivityIds = signal<Set<string>>(new Set());
  readonly activityStatusFilter = signal<'all' | 'pending' | 'completed'>('all');
  readonly filteredActivities = signal<ActivityListItem[]>([]);
  readonly courses = signal<CourseDto[]>([]);
  readonly isInstructor = computed(() => this.authService.isInstructorRole());
  readonly isStudent = computed(() => this.authService.isStudentRole());
  readonly displayedActivities = computed(() =>
    this.isStudent() ? this.filteredActivities() : this.activities()
  );
  readonly activityGroupLabels = computed(() => {
    const labels = new Map<string, string>();
    const activities = this.activities();
    const courses = this.courses();

    const groupCounts = new Map(
      courses.map(course => [
        course.Id,
        (course.ClassGroups ?? []).filter(group => !group.IsMaterialsDistribution).length
      ])
    );

    const groupsByKey = new Map<string, Set<string>>();
    activities.forEach(activity => {
      const key = this.activitySignature(activity);
      const set = groupsByKey.get(key) ?? new Set<string>();
      set.add(activity.classGroupName || 'Turma nao informada');
      groupsByKey.set(key, set);
    });

    groupsByKey.forEach((groupSet, key) => {
      const [courseId] = key.split('|');
      const totalGroups = groupCounts.get(courseId) ?? 0;
      const label = totalGroups > 0 && groupSet.size >= totalGroups
        ? 'Todas as turmas'
        : Array.from(groupSet).join(', ');
      labels.set(key, label);
    });

    return labels;
  });

  constructor() {
    const user = this.authService.currentUser();
    if (!user) {
      this.error.set('Usuario nao identificado.');
      this.loading.set(false);
      return;
    }

    if (this.authService.isInstructorRole()) {
      forkJoin({
        activities: this.service.getActivities(),
        courses: this.coursesService.getCoursesDto()
      })
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: ({ activities, courses }) => {
            this.activities.set(activities);
            this.courses.set(courses);
            this.completedActivityIds.set(new Set());
            this.filteredActivities.set(activities);
            this.error.set(null);
            this.loading.set(false);
          },
          error: () => {
            this.error.set('Nao foi possivel carregar as atividades.');
            this.loading.set(false);
          }
        });
      return;
    }

    forkJoin({
      courses: this.coursesService.getCoursesDto(),
      subscriptions: this.subscriptionsService.getByStudent(user.id)
    })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        switchMap(({ courses, subscriptions }) => {
          this.courses.set(courses);
          this.activityStatusFilter.set('all');

          const allowedGroupIds = this.resolveStudentClassGroups(courses, user.id, subscriptions.map(item => item.CourseId));
          if (allowedGroupIds.length === 0) {
            return of({ activities: [] as ActivityListItem[], completedIds: new Set<string>() });
          }

          return forkJoin({
            activities: this.loadActivitiesForGroups(allowedGroupIds),
            submissions: this.loadSubmissionsForGroups(user.id, allowedGroupIds)
          }).pipe(
            map(({ activities, submissions }) => {
              const completedIds = new Set(
                submissions.filter(submission => submission.Status >= 2).map(submission => submission.ActivityId)
              );
              return { activities, completedIds };
            })
          );
        })
      )
      .subscribe({
        next: ({ activities, completedIds }) => {
          this.activities.set(activities);
          this.completedActivityIds.set(completedIds);
          this.applyActivityFilter();
          this.error.set(null);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Nao foi possivel carregar as atividades.');
          this.loading.set(false);
        }
      });
  }

  trackByActivity(_: number, item: ActivityListItem): string {
    return item.id;
  }

  viewActivity(activity: ActivityListItem): void {
    this.router.navigate(['/courses', activity.courseId, 'activities', activity.id]);
  }

  manageCorrections(activity: ActivityListItem): void {
    if (!activity.id) {
      return;
    }
    this.router.navigate(['/activities', activity.id, 'corrections']);
  }

  activityGroupLabel(activity: ActivityListItem): string {
    return this.activityGroupLabels().get(this.activitySignature(activity)) ?? activity.classGroupName;
  }

  private activitySignature(activity: ActivityListItem): string {
    return [
      activity.courseId,
      activity.title,
      activity.dueDateLabel,
      String(activity.allowLate),
      String(activity.attachments)
    ].join('|');
  }

  setActivityStatusFilter(filter: 'all' | 'pending' | 'completed'): void {
    this.activityStatusFilter.set(filter);
    this.applyActivityFilter();
  }

  private resolveStudentClassGroups(courses: CourseDto[], studentId: string, subscriptionCourseIds: string[]): string[] {
    const allowed = new Set<string>();
    const subscribedCourses = new Set(subscriptionCourseIds);
    const allowedStatus = new Set([1, 2]);

    courses.forEach(course => {
      const groups = course.ClassGroups ?? [];
      groups.forEach(group => {
        if (group.IsMaterialsDistribution) {
          if (subscribedCourses.has(course.Id)) {
            allowed.add(group.Id);
          }
          return;
        }

        const enrollments = group.Enrollments ?? [];
        const hasEnrollment = enrollments.some(
          enrollment => enrollment.StudentId === studentId && allowedStatus.has(enrollment.Status)
        );
        if (hasEnrollment) {
          allowed.add(group.Id);
        }
      });
    });

    return Array.from(allowed);
  }

  private loadActivitiesForGroups(classGroupIds: string[]) {
    if (classGroupIds.length === 1) {
      return this.service.getActivities({ ClassGroupId: classGroupIds[0], VisibleToStudents: true });
    }

    const requests = classGroupIds.map(classGroupId =>
      this.service.getActivities({ ClassGroupId: classGroupId, VisibleToStudents: true })
    );
    return forkJoin(requests).pipe(map(items => items.flat()));
  }

  private loadSubmissionsForGroups(studentId: string, classGroupIds: string[]) {
    const requests = classGroupIds.map(classGroupId =>
      this.submissionsService.getSubmissions({ StudentId: studentId, ClassGroupId: classGroupId, PageSize: 200 })
    );
    return forkJoin(requests).pipe(map(results => results.flatMap(result => result.items)));
  }

  private applyActivityFilter(): void {
    if (!this.authService.isStudentRole()) {
      this.filteredActivities.set(this.activities());
      return;
    }

    const filter = this.activityStatusFilter();
    const completedIds = this.completedActivityIds();
    const items = this.activities();

    if (filter === 'all') {
      this.filteredActivities.set(items);
      return;
    }

    const filtered = items.filter(activity =>
      filter === 'completed' ? completedIds.has(activity.id) : !completedIds.has(activity.id)
    );
    this.filteredActivities.set(filtered);
  }
}

