import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ToastrService } from 'ngx-toastr';
import { forkJoin, of } from 'rxjs';
import { filter, map, switchMap } from 'rxjs/operators';

import { ClassGroupsService } from '../../core/services/class-groups.service';
import { CoursesService } from '../../core/services/courses.service';
import { ForumService } from '../../core/services/forum.service';
import { AuthService } from '../../core/services/auth.service';
import { ClassGroupDto, ClassEnrollmentDto, CourseDto } from '../../core/api/courses.api';
import { ForumThreadCreatePayload, ForumThreadListItem } from '../../core/api/forum.api';

@Component({
  selector: 'app-class-group-manage',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './class-group-manage.component.html',
  styleUrl: './class-group-manage.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClassGroupManageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);
  private readonly classGroupsService = inject(ClassGroupsService);
  private readonly coursesService = inject(CoursesService);
  private readonly forumService = inject(ForumService);
  private readonly authService = inject(AuthService);
  private readonly toastr = inject(ToastrService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly threadsLoading = signal(false);
  readonly classGroup = signal<ClassGroupDto | null>(null);
  readonly course = signal<CourseDto | null>(null);
  readonly threads = signal<ForumThreadListItem[]>([]);
  readonly isThreadFormVisible = signal(false);
  readonly isThreadSubmitting = signal(false);

  readonly capacity = computed(() => this.classGroup()?.Capacity ?? 0);
  readonly approved = computed(() => this.classGroup()?.ApprovedEnrollments ?? 0);
  readonly pending = computed(() => this.classGroup()?.PendingEnrollments ?? 0);
  readonly available = computed(() => {
    const cap = this.capacity();
    const approved = this.approved();
    return Math.max(cap - approved, 0);
  });
  readonly occupancyPercent = computed(() => {
    const cap = this.capacity();
    const approved = this.approved();
    if (!cap) {
      return 0;
    }
    return Math.min(100, Math.round((approved / cap) * 100));
  });
  readonly enrollments = computed<ClassEnrollmentDto[]>(() => this.classGroup()?.Enrollments ?? []);

  readonly threadForm = this.fb.group({
    title: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(180)]),
    description: this.fb.control<string | null>('', [Validators.maxLength(1200)]),
    isPinned: this.fb.nonNullable.control(false)
  });

  private readonly enrollmentStatusLabels: Record<number, string> = {
    1: 'Pendente',
    2: 'Aprovada',
    3: 'Recusada',
    4: 'Cancelada'
  };

  constructor() {
    this.route.paramMap
      .pipe(
        map(params => params.get('classGroupId')),
        filter((classGroupId): classGroupId is string => Boolean(classGroupId)),
        switchMap(classGroupId =>
          this.classGroupsService.getClassGroupById(classGroupId).pipe(
            switchMap(group =>
              forkJoin({
                group: of(group),
                course: this.coursesService.getCourseById(group.CourseId)
              })
            ),
            map(result => ({ ...result, classGroupId }))
          )
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: ({ classGroupId, group, course }) => {
          this.classGroup.set(group);
          this.course.set(course);
          this.error.set(null);
          this.loading.set(false);
          this.reloadThreads();
        },
        error: () => {
          this.classGroup.set(null);
          this.course.set(null);
          this.error.set('Nao foi possivel carregar os dados da turma.');
          this.loading.set(false);
        }
      });
  }

  openThreadForm(): void {
    this.isThreadFormVisible.set(true);
    this.threadForm.reset({ title: '', description: '', isPinned: false });
  }

  cancelThreadCreation(): void {
    this.threadForm.reset({ title: '', description: '', isPinned: false });
    this.isThreadFormVisible.set(false);
  }

  statusLabel(status: number): string {
    return this.enrollmentStatusLabels[status] ?? 'Desconhecido';
  }

  statusBadgeClass(status: number): string {
    switch (status) {
      case 1:
        return 'badge--warning';
      case 2:
        return 'badge--success';
      case 3:
        return 'badge--danger';
      case 4:
        return 'badge--neutral';
      default:
        return 'badge--neutral';
    }
  }

  submitThread(): void {
    if (this.threadForm.invalid) {
      this.threadForm.markAllAsTouched();
      return;
    }

    const group = this.classGroup();
    const currentUser = this.authService.currentUser();

    if (!group) {
      this.toastr.error('Carregue os dados da turma antes de criar um topico.');
      return;
    }

    if (!currentUser) {
      this.toastr.error('Usuario nao autenticado.');
      return;
    }

    const raw = this.threadForm.getRawValue();
    const title = raw.title.trim();
    if (!title) {
      this.threadForm.controls.title.setErrors({ required: true });
      this.threadForm.controls.title.markAsTouched();
      return;
    }

    const payload: ForumThreadCreatePayload = {
      ClassGroupId: group.Id,
      CreatedById: currentUser.id,
      Title: title,
      Description: raw.description?.trim() || undefined,
      IsPinned: raw.isPinned
    };

    this.isThreadSubmitting.set(true);

    this.forumService
      .createThread(payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastr.success('Topico criado com sucesso.');
          this.isThreadSubmitting.set(false);
          this.isThreadFormVisible.set(false);
          this.threadForm.reset({ title: '', description: '', isPinned: false });
          this.reloadThreads();
        },
        error: () => {
          this.toastr.error('Nao foi possivel criar o topico.');
          this.isThreadSubmitting.set(false);
        }
      });
  }

  navigateBack(): void {
    this.router.navigate(['/class-groups']);
  }

  private reloadThreads(): void {
    const group = this.classGroup();
    const course = this.course();

    if (!group || !course) {
      return;
    }

    const lookup = new Map<string, string>([[course.Id, course.Title]]);
    this.threadsLoading.set(true);

    this.forumService
      .getThreadsByClassGroup(lookup, group.Id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: threads => {
          this.threads.set(threads);
          this.threadsLoading.set(false);
        },
        error: () => {
          this.toastr.error('Nao foi possivel carregar os topicos da turma.');
          this.threads.set([]);
          this.threadsLoading.set(false);
        }
      });
  }

  trackByThread(_: number, item: ForumThreadListItem): string {
    return item.id;
  }
}
