import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter, map, switchMap } from 'rxjs';

import { CoursesService } from '../../core/services/courses.service';
import { CourseDto, ClassGroupDto } from '../../core/api/courses.api';
import { CourseActivitiesComponent } from './course-activities.component';
import { CourseContentsComponent } from './course-contents.component';

@Component({
  selector: 'app-course-manage',
  standalone: true,
  imports: [CommonModule, RouterLink, CourseActivitiesComponent, CourseContentsComponent],
  templateUrl: './course-manage.component.html',
  styleUrl: './course-manage.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CourseManageComponent {
  private readonly modeLabels: Record<number, string> = {
    1: 'Turmas interativas',
    2: 'Distribuição de materiais'
  };

  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly coursesService = inject(CoursesService);
  private readonly router = inject(Router);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly course = signal<CourseDto | null>(null);

  readonly totalCapacity = computed(() => {
    const current = this.course();
    if (!current) {
      return 0;
    }
    return current.ClassGroups.reduce((sum, group) => sum + (group.Capacity ?? 0), 0);
  });

  readonly totalApproved = computed(() => {
    const current = this.course();
    if (!current) {
      return 0;
    }
    return current.ClassGroups.reduce((sum, group) => sum + (group.ApprovedEnrollments ?? 0), 0);
  });

  readonly totalPending = computed(() => {
    const current = this.course();
    if (!current) {
      return 0;
    }
    return current.ClassGroups.reduce((sum, group) => sum + (group.PendingEnrollments ?? 0), 0);
  });

  readonly materialsClassGroup = computed(() => {
    const current = this.course();
    if (!current) {
      return null;
    }
    return current.ClassGroups.find(group => group.IsMaterialsDistribution) ?? null;
  });

  constructor() {
    this.route.paramMap
      .pipe(
        map(params => params.get('courseId')),
        filter((courseId): courseId is string => Boolean(courseId)),
        switchMap(courseId => this.coursesService.getCourseById(courseId)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: course => {
          this.course.set(course);
          this.error.set(null);
          this.loading.set(false);
        },
        error: () => {
          this.course.set(null);
          this.error.set('Nao foi possivel carregar os detalhes do curso.');
          this.loading.set(false);
        }
      });
  }

  statusLabel(course: CourseDto | null): string {
    if (!course) {
      return '';
    }
    return course.IsPublished ? 'Publicado' : 'Rascunho';
  }

  modeLabel(course: CourseDto | null): string {
    if (!course) {
      return '';
    }
    return this.modeLabels[course.Mode] ?? 'Modalidade desconhecida';
  }

  featureLabel(enabled: boolean): string {
    return enabled ? 'Ativo' : 'Desativado';
  }

  publishedLabel(course: CourseDto | null): string {
    if (!course) {
      return 'Nunca publicado';
    }
    if (!course.IsPublished || !course.PublishedAt) {
      return 'Nunca publicado';
    }
    return new Date(course.PublishedAt).toLocaleDateString('pt-BR');
  }

  createdLabel(course: CourseDto | null): string {
    if (!course) {
      return '';
    }
    return new Date(course.CreatedAt).toLocaleDateString('pt-BR');
  }

  goToCreateClassGroup(): void {
    const course = this.course();
    this.router.navigate(['/class-groups/create'], {
      queryParams: course?.Id ? { courseId: course.Id } : undefined
    });
  }

  classGroupOccupancy(group: ClassGroupDto): number {
    if (!group.Capacity) {
      return 0;
    }
    return Math.min(100, Math.round((group.ApprovedEnrollments / group.Capacity) * 100));
  }

  navigateToClassGroup(group: ClassGroupDto): void {
    if (!group?.Id) {
      return;
    }
    this.router.navigate(['/class-groups', group.Id, 'manage']);
  }
}
