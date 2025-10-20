import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

import { CoursesService } from '../../core/services/courses.service';
import { ClassGroupsService } from '../../core/services/class-groups.service';
import { ClassGroupCreatePayload, CourseDto } from '../../core/api/courses.api';

@Component({
  selector: 'app-class-group-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './class-group-create.component.html',
  styleUrl: './class-group-create.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClassGroupCreateComponent {
  private readonly fb = inject(FormBuilder);
  private readonly coursesService = inject(CoursesService);
  private readonly classGroupsService = inject(ClassGroupsService);
  private readonly toastr = inject(ToastrService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly loadingCourses = signal(true);
  readonly loadError = signal<string | null>(null);
  private readonly courses = signal<readonly CourseDto[]>([]);
  readonly interactiveCourses = computed(() => this.courses().filter(course => course.Mode === 1));
  readonly isSubmitting = signal(false);

  readonly form = this.fb.group({
    courseId: this.fb.nonNullable.control('', [Validators.required]),
    name: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(120)]),
    capacity: this.fb.nonNullable.control(30, [Validators.required, Validators.min(1)]),
    requiresApproval: this.fb.nonNullable.control(false),
    requiresEnrollmentCode: this.fb.nonNullable.control(false),
    enrollmentCode: this.fb.control<string | null>({ value: '', disabled: true }),
    enableChat: this.fb.nonNullable.control(true),
    enrollmentOpensAt: this.fb.control<string | null>(null),
    enrollmentClosesAt: this.fb.control<string | null>(null),
    startsAt: this.fb.control<string | null>(null),
    endsAt: this.fb.control<string | null>(null)
  });

  constructor() {
    this.fetchCourses();
    this.setupEnrollmentCodeControl();
  }

  get courseIdControl(): FormControl<string> {
    return this.form.get('courseId') as FormControl<string>;
  }

  get requiresEnrollmentCodeControl(): FormControl<boolean> {
    return this.form.get('requiresEnrollmentCode') as FormControl<boolean>;
  }

  get enrollmentCodeControl(): FormControl<string | null> {
    return this.form.get('enrollmentCode') as FormControl<string | null>;
  }

  get hasInteractiveCourses(): boolean {
    return this.interactiveCourses().length > 0;
  }

  isInvalid(controlName: string): boolean {
    const control = this.form.get(controlName);
    return !!control && control.invalid && (control.dirty || control.touched);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.buildPayload();

    if (!this.interactiveCourses().some(course => course.Id === payload.CourseId)) {
      this.toastr.error('Selecione um curso interativo valido.');
      return;
    }

    this.isSubmitting.set(true);

    this.classGroupsService
      .createClassGroup(payload)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSubmitting.set(false))
      )
      .subscribe({
        next: () => {
          this.toastr.success('Turma criada com sucesso.');
          this.router.navigate(['/class-groups']);
        },
        error: error => {
          console.error('Falha ao criar turma', error);
          this.toastr.error('Nao foi possivel criar a turma. Tente novamente.');
        }
      });
  }

  trackByCourseId(_: number, course: CourseDto): string {
    return course.Id;
  }

  private fetchCourses(): void {
    this.coursesService
      .getCoursesDto()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: courses => {
          this.courses.set(courses);
          this.loadingCourses.set(false);
          this.loadError.set(null);
        },
        error: () => {
          this.courses.set([]);
          this.loadingCourses.set(false);
          this.loadError.set('Nao foi possivel carregar os cursos.');
        }
      });
  }

  private buildPayload(): ClassGroupCreatePayload {
    const raw = this.form.getRawValue();
    const requiresCode = raw.requiresEnrollmentCode;
    const enrollmentCode = String(raw.enrollmentCode ?? '').trim();

    return {
      CourseId: raw.courseId,
      Name: raw.name.trim(),
      Description: undefined,
      Capacity: Number.isFinite(Number(raw.capacity)) ? Math.max(1, Math.trunc(Number(raw.capacity))) : 1,
      RequiresApproval: raw.requiresApproval,
      RequiresEnrollmentCode: requiresCode,
      EnrollmentCode: requiresCode && enrollmentCode ? enrollmentCode : undefined,
      EnableChat: raw.enableChat,
      IsMaterialsDistribution: false,
      EnrollmentOpensAt: this.toIsoString(raw.enrollmentOpensAt),
      EnrollmentClosesAt: this.toIsoString(raw.enrollmentClosesAt),
      StartsAt: this.toIsoString(raw.startsAt),
      EndsAt: this.toIsoString(raw.endsAt)
    };
  }

  private setupEnrollmentCodeControl(): void {
    const syncState = (requiresCode: boolean | null) => {
      const isRequired = Boolean(requiresCode);

      if (isRequired) {
        this.enrollmentCodeControl.enable({ emitEvent: false });
        this.enrollmentCodeControl.setValidators([Validators.required, Validators.maxLength(120)]);
      } else {
        this.enrollmentCodeControl.disable({ emitEvent: false });
        this.enrollmentCodeControl.reset('', { emitEvent: false });
        this.enrollmentCodeControl.clearValidators();
      }

      this.enrollmentCodeControl.updateValueAndValidity({ emitEvent: false });
    };

    syncState(this.requiresEnrollmentCodeControl.value);
    this.requiresEnrollmentCodeControl.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(syncState);
  }

  private toIsoString(value: unknown): string | undefined {
    if (!value) {
      return undefined;
    }

    const parsed = new Date(String(value));
    return Number.isNaN(parsed.getTime()) ? undefined : parsed.toISOString();
  }
}
