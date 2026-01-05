import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';

import { ActivitiesService } from '../../core/services/activities.service';
import { ActivityListItem } from '../../core/api/activities.api';
import { CoursesService } from '../../core/services/courses.service';
import { CourseDto } from '../../core/api/courses.api';

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
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly activities = signal<ActivityListItem[]>([]);
  readonly courses = signal<CourseDto[]>([]);
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
    forkJoin({
      activities: this.service.getActivities(),
      courses: this.coursesService.getCoursesDto()
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ activities, courses }) => {
          this.activities.set(activities);
          this.courses.set(courses);
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
}

