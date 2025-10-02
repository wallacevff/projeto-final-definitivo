import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

import { CoursesService, CreateCoursePayload } from '../../core/services/courses.service';

type CourseMode = 'interactive' | 'distribution';

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
  private readonly toastr = inject(ToastrService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly isSubmitting = signal(false);

  readonly categories = [
    { id: 'computacao', label: 'Computacao' },
    { id: 'musica', label: 'Musica' },
    { id: 'matematica', label: 'Matematica' },
    { id: 'portugues', label: 'Lingua Portuguesa' },
    { id: 'gestao', label: 'Gestao e Negocios' }
  ];

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
    categoryId: this.fb.nonNullable.control('', [Validators.required]),
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

    const payload = this.buildPayload();
    this.isSubmitting.set(true);

    this.service
      .createCourse(payload)
      .pipe(takeUntilDestroyed(this.destroyRef), finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => {
          this.toastr.success('Curso criado com sucesso.');
          this.router.navigate(['/courses']);
        },
        error: error => {
          console.error('Falha ao criar curso', error);
          this.toastr.error('Nao foi possivel criar o curso. Tente novamente.');
        }
      });
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

  private buildPayload(): CreateCoursePayload {
    const raw = this.form.getRawValue();
    const modeNumber = raw.mode === 'interactive' ? 1 : 2;

    const classGroups =
      raw.mode === 'interactive'
        ? (raw.classGroups as Array<Record<string, unknown>>).map(group => ({
            Name: String(group['name'] ?? ''),
            Capacity: Number(group['capacity'] ?? 0),
            RequiresApproval: Boolean(group['requiresApproval']),
            RequiresEnrollmentCode: Boolean(group['requiresEnrollmentCode']),
            EnableChat: Boolean(group['enableChat']),
            EnrollmentOpensAt: this.toIsoString(group['enrollmentOpensAt']),
            EnrollmentClosesAt: this.toIsoString(group['enrollmentClosesAt']),
            StartsAt: this.toIsoString(group['startsAt']),
            EndsAt: this.toIsoString(group['endsAt'])
          }))
        : [];

    return {
      Title: raw.title,
      ShortDescription: raw.shortDescription,
      DetailedDescription: raw.detailedDescription ?? undefined,
      Mode: modeNumber,
      CategoryId: raw.categoryId,
      EnableForum: raw.enableForum,
      EnableChat: modeNumber === 1 ? raw.enableChat : false,
      EnrollmentInstructions: raw.enrollmentInstructions ?? undefined,
      IsPublished: raw.isPublished,
      ClassGroups: classGroups
    } satisfies CreateCoursePayload;
  }

  private toIsoString(value: unknown): string | undefined {
    if (!value) {
      return undefined;
    }

    const parsed = new Date(String(value));
    return Number.isNaN(parsed.getTime()) ? undefined : parsed.toISOString();
  }
}










