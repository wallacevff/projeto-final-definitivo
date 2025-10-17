import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin, Observable, of } from 'rxjs';
import { finalize, switchMap } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

import { CoursesService, CreateCoursePayload, UpdateCoursePayload } from '../../core/services/courses.service';
import { ClassGroupsService } from '../../core/services/class-groups.service';
import { ClassGroupCreatePayload } from '../../core/api/courses.api';

type CourseMode = 'interactive' | 'distribution';

interface ClassGroupFormPayload {
  Name: string;
  Description?: string;
  Capacity: number;
  RequiresApproval: boolean;
  RequiresEnrollmentCode: boolean;
  EnrollmentCode?: string;
  EnableChat: boolean;
  EnrollmentOpensAt?: string;
  EnrollmentClosesAt?: string;
  StartsAt?: string;
  EndsAt?: string;
}

interface CourseSubmissionPayload {
  course: CreateCoursePayload;
  classGroups: ClassGroupFormPayload[];
  publish: boolean;
}

@Component({
  selector: 'app-course-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './course-create.component.html',
  styleUrl: './course-create.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CourseCreateComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(CoursesService);
  private readonly classGroupsService = inject(ClassGroupsService);
  private readonly toastr = inject(ToastrService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly isSubmitting = signal(false);

  readonly modeOptions: ReadonlyArray<{ value: CourseMode; label: string; description: string }> = [
    {
      value: 'interactive',
      label: 'Turmas interativas',
      description: 'Limite de alunos por turma, chat ao vivo e acompanhamento em tempo real.'
    },
    {
      value: 'distribution',
      label: 'Distribuicao de material',
      description: 'Conteudos sob demanda, acesso ilimitado e foco em materiais.'
    }
  ];

  readonly form = this.fb.group({
    title: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(120)]),
    shortDescription: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(280)]),
    detailedDescription: this.fb.control('', [Validators.maxLength(4000)]),
    categoryName: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(150)]),
    mode: this.fb.nonNullable.control<CourseMode>('interactive', [Validators.required]),
    enableForum: this.fb.nonNullable.control(true),
    enableChat: this.fb.nonNullable.control(true),
    isPublished: this.fb.nonNullable.control(false),
    enrollmentInstructions: this.fb.control(''),
    classGroups: this.fb.array([this.createClassGroupGroup()])
  });

  readonly isInteractive = signal(this.modeControl.value === 'interactive');

  constructor() {
    this.modeControl.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(mode => {
      this.handleModeChange(mode);
    });

    this.handleModeChange(this.modeControl.value);
  }

  get classGroups(): FormArray<FormGroup> {
    return this.form.get('classGroups') as FormArray<FormGroup>;
  }

  get modeControl(): FormControl<CourseMode> {
    return this.form.get('mode') as FormControl<CourseMode>;
  }

  get categoryNameControl(): FormControl<string> {
    return this.form.get('categoryName') as FormControl<string>;
  }

  get enableChatControl(): FormControl<boolean> {
    return this.form.get('enableChat') as FormControl<boolean>;
  }

  get interactiveMode(): boolean {
    return this.isInteractive();
  }

  handleModeChange(mode: CourseMode): void {
    this.isInteractive.set(mode === 'interactive');
    if (mode === 'interactive') {
      if (this.classGroups.length === 0) {
        this.classGroups.push(this.createClassGroupGroup());
      }

      if (this.enableChatControl.disabled) {
        this.enableChatControl.enable({ emitEvent: false });
      }
    } else {
      this.enableChatControl.setValue(false, { emitEvent: false });
      this.enableChatControl.disable({ emitEvent: false });

      while (this.classGroups.length > 0) {
        this.classGroups.removeAt(0, { emitEvent: false });
      }
    }
  }

  addClassGroup(): void {
    this.classGroups.push(this.createClassGroupGroup());
  }

  removeClassGroup(index: number): void {
    this.classGroups.removeAt(index);
  }

  isInvalid(controlName: string): boolean {
    const control = this.form.get(controlName);
    return !!control && control.invalid && (control.dirty || control.touched);
  }

  isGroupInvalid(group: FormGroup, controlName: string): boolean {
    const control = group.get(controlName);
    return !!control && control.invalid && (control.dirty || control.touched);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const submission = this.buildSubmissionPayload();
    this.isSubmitting.set(true);

    this.handleCourseCreation(submission);
  }

  private handleCourseCreation(submission: CourseSubmissionPayload): void {
    const { course, classGroups, publish } = submission;

    this.service
      .createCourse(course)
      .pipe(
        switchMap(createdCourse =>
          this.runPostCreationOperations(createdCourse.Id, course, classGroups, publish)
        ),
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSubmitting.set(false))
      )
      .subscribe({
        next: () => {
          const message = publish ? 'Curso criado e publicado com sucesso.' : 'Curso criado com sucesso.';
          this.toastr.success(message);
          this.router.navigate(['/courses']);
        },
        error: error => {
          console.error('Falha ao criar curso', error);
          this.toastr.error('Nao foi possivel criar o curso. Tente novamente.');
        }
      });
  }

  private runPostCreationOperations(
    courseId: string,
    coursePayload: CreateCoursePayload,
    classGroups: ClassGroupFormPayload[],
    publish: boolean
  ) {
    const operations: Observable<unknown>[] = [];

    if (classGroups.length) {
      const payloads: ClassGroupCreatePayload[] = classGroups.map(group => ({
        ...group,
        CourseId: courseId
      }));
      operations.push(this.classGroupsService.createMany(payloads));
    }

    if (publish) {
      const updatePayload: UpdateCoursePayload = {
        Title: coursePayload.Title,
        ShortDescription: coursePayload.ShortDescription,
        DetailedDescription: coursePayload.DetailedDescription,
        Mode: coursePayload.Mode,
        CategoryName: coursePayload.CategoryName,
        EnableForum: coursePayload.EnableForum,
        EnableChat: coursePayload.EnableChat,
        IsPublished: true,
        EnrollmentInstructions: coursePayload.EnrollmentInstructions,
        ThumbnailMediaId: coursePayload.ThumbnailMediaId
      };
      operations.push(this.service.updateCourse(courseId, updatePayload));
    }

    if (!operations.length) {
      return of(null);
    }

    return forkJoin(operations);
  }

  private createClassGroupGroup(): FormGroup {
    return this.fb.group({
      name: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(120)]),
      capacity: this.fb.nonNullable.control(30, [Validators.required, Validators.min(1)]),
      requiresApproval: this.fb.nonNullable.control(false),
      requiresEnrollmentCode: this.fb.nonNullable.control(false),
      enableChat: this.fb.nonNullable.control(true),
      enrollmentOpensAt: this.fb.control<string | null>(null),
      enrollmentClosesAt: this.fb.control<string | null>(null),
      startsAt: this.fb.control<string | null>(null),
      endsAt: this.fb.control<string | null>(null)
    });
  }

  private buildSubmissionPayload(): CourseSubmissionPayload {
    const raw = this.form.getRawValue();
    const modeNumber = raw.mode === 'interactive' ? 1 : 2;

    const course: CreateCoursePayload = {
      Title: raw.title.trim(),
      ShortDescription: raw.shortDescription.trim(),
      DetailedDescription: raw.detailedDescription?.trim() || undefined,
      Mode: modeNumber,
      CategoryName: String(raw.categoryName ?? '').trim(),
      EnableForum: raw.enableForum,
      EnableChat: modeNumber === 1 ? raw.enableChat : false,
      EnrollmentInstructions: raw.enrollmentInstructions?.trim() || undefined
    };

    const classGroups =
      modeNumber === 1
        ? this.buildClassGroupPayloads(raw.classGroups as Array<Record<string, unknown>>)
        : [];

    return {
      course,
      classGroups,
      publish: raw.isPublished
    };
  }

  private buildClassGroupPayloads(groups: Array<Record<string, unknown>>): ClassGroupFormPayload[] {
    return groups.map(group => {
      const rawCapacity = Number(group['capacity'] ?? 0);
      const capacity = Number.isFinite(rawCapacity) ? Math.max(1, Math.trunc(rawCapacity)) : 1;

      return {
        Name: String(group['name'] ?? '').trim(),
        Description: undefined,
        Capacity: capacity,
        RequiresApproval: Boolean(group['requiresApproval']),
        RequiresEnrollmentCode: Boolean(group['requiresEnrollmentCode']),
        EnrollmentCode: undefined,
        EnableChat: Boolean(group['enableChat']),
        EnrollmentOpensAt: this.toIsoString(group['enrollmentOpensAt']),
        EnrollmentClosesAt: this.toIsoString(group['enrollmentClosesAt']),
        StartsAt: this.toIsoString(group['startsAt']),
        EndsAt: this.toIsoString(group['endsAt'])
      };
    });
  }

  private toIsoString(value: unknown): string | undefined {
    if (!value) {
      return undefined;
    }

    const parsed = new Date(String(value));
    return Number.isNaN(parsed.getTime()) ? undefined : parsed.toISOString();
  }
}













