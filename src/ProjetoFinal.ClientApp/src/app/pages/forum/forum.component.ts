import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ToastrService } from 'ngx-toastr';

import { ForumService } from '../../core/services/forum.service';
import { CoursesService } from '../../core/services/courses.service';
import { ForumThreadCreatePayload, ForumThreadListItem } from '../../core/api/forum.api';
import { AuthService } from '../../core/services/auth.service';
import { ClassGroupDto, CourseDto } from '../../core/api/courses.api';

@Component({
  selector: 'app-forum',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './forum.component.html',
  styleUrl: './forum.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ForumComponent {
  private readonly forumService = inject(ForumService);
  private readonly coursesService = inject(CoursesService);
  private readonly authService = inject(AuthService);
  private readonly toastr = inject(ToastrService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private courseLookup = new Map<string, string>();

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly threads = signal<ForumThreadListItem[]>([]);
  readonly courses = signal<CourseDto[]>([]);
  readonly isCreateVisible = signal(false);
  readonly isSubmitting = signal(false);

  readonly form = this.fb.group({
    courseId: this.fb.nonNullable.control('', [Validators.required]),
    classGroupId: this.fb.nonNullable.control('', [Validators.required]),
    title: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(180)]),
    description: this.fb.control<string | null>('', [Validators.maxLength(1200)]),
    isPinned: this.fb.nonNullable.control(false)
  });

  readonly availableClassGroups = computed<ClassGroupDto[]>(() => {
    const courseId = this.form.controls.courseId.value;
    const course = this.courses().find(item => item.Id === courseId);
    const groups = course?.ClassGroups ?? [];
    return groups.filter(group => !group.IsMaterialsDistribution);
  });

  readonly hasClassGroups = computed(() =>
    this.courses().some(course =>
      (course.ClassGroups ?? []).some(group => !group.IsMaterialsDistribution)
    )
  );

  constructor() {
    this.loadCourses();

    this.form.controls.courseId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.syncClassGroupSelection());
  }

  trackByThread(_: number, item: ForumThreadListItem): string {
    return item.id;
  }

  openCreateForm(): void {
    if (!this.hasClassGroups()) {
      this.toastr.warning('Nenhum curso com turmas disponiveis para criar topico.');
      return;
    }

    this.isCreateVisible.set(true);
    const firstCourse = this.findFirstCourseWithGroups();
    const initialCourseId = firstCourse?.Id ?? '';

    this.form.reset({
      courseId: initialCourseId,
      classGroupId: firstCourse?.ClassGroups[0]?.Id ?? '',
      title: '',
      description: '',
      isPinned: false
    });
    this.syncClassGroupSelection();
  }

  cancelCreate(): void {
    this.isCreateVisible.set(false);
    this.form.reset({
      courseId: '',
      classGroupId: '',
      title: '',
      description: '',
      isPinned: false
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const currentUser = this.authService.currentUser();
    if (!currentUser) {
      this.toastr.error('Usuario nao autenticado.');
      return;
    }

    const raw = this.form.getRawValue();
    const classGroups = this.availableClassGroups();
    if (!classGroups.some(group => group.Id === raw.classGroupId)) {
      this.toastr.error('Selecione uma turma valida para criar o topico.');
      return;
    }

    const payload: ForumThreadCreatePayload = {
      ClassGroupId: raw.classGroupId,
      CreatedById: currentUser.id,
      Title: raw.title.trim(),
      Description: raw.description?.trim() || undefined,
      IsPinned: raw.isPinned
    };

    this.isSubmitting.set(true);

    this.forumService
      .createThread(payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastr.success('Topico criado com sucesso.');
          this.isSubmitting.set(false);
          this.cancelCreate();
          this.loadThreads();
        },
        error: () => {
          this.toastr.error('Nao foi possivel criar o topico.');
          this.isSubmitting.set(false);
        }
      });
  }

  private loadCourses(): void {
    this.coursesService
      .getCoursesDto()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: courses => {
          this.courses.set(courses);
          this.syncInitialSelections(courses);
          this.loadThreads();
        },
        error: () => {
          this.error.set('Nao foi possivel carregar os cursos.');
          this.loading.set(false);
        }
      });
  }

  private loadThreads(): void {
    const lookup = new Map(this.courses().map(course => [course.Id, course.Title] as const));
    this.courseLookup = lookup;

    this.forumService
      .getThreads(lookup)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: threads => {
          this.threads.set(threads);
          this.error.set(null);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Nao foi possivel carregar os topicos do forum.');
          this.loading.set(false);
        }
      });
  }

  private syncInitialSelections(courses: CourseDto[]): void {
    if (!courses.length) {
      this.form.reset({ courseId: '', classGroupId: '' });
      return;
    }

    const firstAvailable = this.findFirstCourseWithGroups();
    if (firstAvailable) {
      this.form.controls.courseId.setValue(firstAvailable.Id, { emitEvent: false });
      const firstGroup = (firstAvailable.ClassGroups ?? []).find(group => !group.IsMaterialsDistribution);
      this.form.controls.classGroupId.setValue(firstGroup?.Id ?? '', { emitEvent: false });
    }

    this.syncClassGroupSelection();
  }

  private syncClassGroupSelection(): void {
    const groups = this.availableClassGroups();
    if (!groups.length) {
      this.form.controls.classGroupId.setValue('', { emitEvent: false });
      return;
    }

    const currentGroupId = this.form.controls.classGroupId.value;
    if (!currentGroupId || !groups.some(group => group.Id === currentGroupId)) {
      this.form.controls.classGroupId.setValue(groups[0].Id, { emitEvent: false });
    }
  }

  private findFirstCourseWithGroups(): CourseDto | undefined {
    return this.courses().find(course =>
      (course.ClassGroups ?? []).some(group => !group.IsMaterialsDistribution)
    );
  }
}
