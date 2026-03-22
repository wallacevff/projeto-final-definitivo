import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter, map, switchMap } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

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
  private readonly toastr = inject(ToastrService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly course = signal<CourseDto | null>(null);
  readonly publishingCourse = signal(false);
  readonly savingDraftDetails = signal(false);
  readonly draftTitle = signal('');
  readonly draftCategory = signal('');

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
          this.draftTitle.set(course.Title ?? '');
          this.draftCategory.set(course.CategoryName ?? '');
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

  publishCourse(): void {
    const current = this.course();
    if (!current || current.IsPublished || this.publishingCourse()) {
      return;
    }

    this.publishingCourse.set(true);
    this.coursesService
      .updateCourse(current.Id, {
        Title: current.Title,
        ShortDescription: current.ShortDescription,
        DetailedDescription: current.DetailedDescription ?? undefined,
        Mode: current.Mode,
        CategoryName: current.CategoryName,
        EnableForum: current.EnableForum,
        EnableChat: current.EnableChat,
        IsPublished: true,
        EnrollmentInstructions: current.EnrollmentInstructions ?? undefined,
        ThumbnailMediaId: current.ThumbnailMediaId ?? undefined
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.publishingCourse.set(false))
      )
      .subscribe({
        next: () => {
          this.course.set({
            ...current,
            IsPublished: true,
            PublishedAt: current.PublishedAt ?? new Date().toISOString()
          });
          this.toastr.success('Curso publicado com sucesso.');
        },
        error: () => {
          this.toastr.error('Nao foi possivel publicar o curso.');
        }
      });
  }

  onDraftTitleChange(value: string): void {
    this.draftTitle.set(value ?? '');
  }

  onDraftCategoryChange(value: string): void {
    this.draftCategory.set(value ?? '');
  }

  saveDraftDetails(): void {
    const current = this.course();
    if (!current || current.IsPublished || this.savingDraftDetails()) {
      return;
    }

    const title = (this.draftTitle() ?? '').trim();
    const category = (this.draftCategory() ?? '').trim();
    if (!title || !category) {
      this.toastr.error('Informe titulo e categoria para salvar o rascunho.');
      return;
    }

    this.savingDraftDetails.set(true);
    this.coursesService
      .updateCourse(current.Id, {
        Title: title,
        ShortDescription: current.ShortDescription,
        DetailedDescription: current.DetailedDescription ?? undefined,
        Mode: current.Mode,
        CategoryName: category,
        EnableForum: current.EnableForum,
        EnableChat: current.EnableChat,
        IsPublished: false,
        EnrollmentInstructions: current.EnrollmentInstructions ?? undefined,
        ThumbnailMediaId: current.ThumbnailMediaId ?? undefined
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.savingDraftDetails.set(false))
      )
      .subscribe({
        next: () => {
          this.course.set({
            ...current,
            Title: title,
            CategoryName: category
          });
          this.toastr.success('Rascunho atualizado com sucesso.');
        },
        error: () => {
          this.toastr.error('Nao foi possivel salvar as alteracoes do rascunho.');
        }
      });
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
