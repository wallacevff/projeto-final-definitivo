import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  Input,
  computed,
  inject,
  signal
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

import { ActivitiesService } from '../../core/services/activities.service';
import { ActivityListItem, ActivityCreatePayload } from '../../core/api/activities.api';
import { AuthService } from '../../core/services/auth.service';
import { CourseDto } from '../../core/api/courses.api';
import { MediaResource } from '../../core/api/media.api';
import { MediaService } from '../../core/services/media.service';

type AttachmentStatus = 'uploading' | 'ready' | 'error';

interface AttachmentDraft {
  id: string;
  fileName: string;
  status: AttachmentStatus;
  caption: string;
  media?: MediaResource;
}

@Component({
  selector: 'app-course-activities',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './course-activities.component.html',
  styleUrl: './course-activities.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CourseActivitiesComponent {
  private readonly fb = inject(FormBuilder);
  private readonly activitiesService = inject(ActivitiesService);
  private readonly mediaService = inject(MediaService);
  private readonly authService = inject(AuthService);
  private readonly toastr = inject(ToastrService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly courseState = signal<CourseDto | null>(null);
  private currentCourseId: string | null = null;

  readonly selectedCourse = computed(() => this.courseState());
  readonly activities = signal<ActivityListItem[]>([]);
  readonly activitiesLoading = signal(false);
  readonly formVisible = signal(false);
  readonly isSubmitting = signal(false);
  readonly attachments = signal<AttachmentDraft[]>([]);

  readonly availableGroups = computed(() => {
    const course = this.courseState();
    if (!course) {
      return [];
    }
    return (course.ClassGroups ?? []).filter(group => !group.IsMaterialsDistribution);
  });

  readonly hasAvailableGroups = computed(() => this.availableGroups().length > 0);
  readonly hasUploadingAttachments = computed(() =>
    this.attachments().some(item => item.status === 'uploading')
  );

  readonly form = this.fb.group({
    classGroupId: this.fb.control('', { validators: [Validators.required] }),
    title: this.fb.control('', { validators: [Validators.required, Validators.maxLength(180)] }),
    description: this.fb.control('', { validators: [Validators.required, Validators.maxLength(4000)] }),
    availableAt: this.fb.control<string | null>(null),
    dueDate: this.fb.control<string | null>(null),
    maxScore: this.fb.control<string | null>(null),
    allowLateSubmissions: this.fb.control(false),
    latePenaltyPercentage: this.fb.control<string | null>({ value: null, disabled: true }),
    visibleToStudents: this.fb.control(true)
  });

  @Input({ required: true })
  set course(value: CourseDto | null) {
    this.courseState.set(value);
    this.onCourseChanged(value);
  }

  constructor() {
    this.form.controls.allowLateSubmissions.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(value => this.syncLatePenaltyControl(Boolean(value)));
  }

  toggleForm(): void {
    const visible = !this.formVisible();
    this.formVisible.set(visible);
    if (visible) {
      this.resetForm();
    }
  }

  handleFiles(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = input.files;
    if (!files?.length) {
      return;
    }

    Array.from(files).forEach(file => this.uploadFile(file));
    input.value = '';
  }

  removeAttachment(id: string): void {
    this.attachments.update(list => list.filter(item => item.id !== id));
  }

  updateCaption(id: string, value: string): void {
    this.attachments.update(list =>
      list.map(item => (item.id === id ? { ...item, caption: value } : item))
    );
  }

  trackByActivity(_: number, item: ActivityListItem): string {
    return item.id;
  }

  trackByAttachment(_: number, item: AttachmentDraft): string {
    return item.id;
  }

  submit(): void {
    const course = this.courseState();
    const currentUser = this.authService.currentUser();
    if (!course || !currentUser) {
      this.toastr.error('Usuario nao autenticado ou curso invalido.');
      return;
    }

    if (!this.hasAvailableGroups()) {
      this.toastr.warning('Cadastre uma turma interativa antes de criar atividades.');
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (this.hasUploadingAttachments()) {
      this.toastr.info('Aguarde o envio dos anexos antes de salvar.');
      return;
    }

    const classGroupId = this.form.controls.classGroupId.value ?? '';
    if (!classGroupId) {
      this.toastr.error('Selecione uma turma para vincular a atividade.');
      return;
    }

    const allowLate = Boolean(this.form.controls.allowLateSubmissions.value);
    const payload: ActivityCreatePayload = {
      CourseId: course.Id,
      ClassGroupId: classGroupId,
      CreatedById: currentUser.id,
      Title: (this.form.controls.title.value ?? '').trim(),
      Description: (this.form.controls.description.value ?? '').trim(),
      AvailableAt: this.toIsoString(this.form.controls.availableAt.value),
      DueDate: this.toIsoString(this.form.controls.dueDate.value),
      MaxScore: this.parseDecimal(this.form.controls.maxScore.value),
      AllowLateSubmissions: allowLate,
      LatePenaltyPercentage: allowLate ? this.parseInteger(this.form.controls.latePenaltyPercentage.value) : undefined,
      VisibleToStudents: Boolean(this.form.controls.visibleToStudents.value),
      Attachments: this.attachments()
        .filter(item => item.status === 'ready' && item.media)
        .map(item => ({
          MediaResourceId: item.media!.Id,
          Caption: item.caption?.trim() || undefined
        }))
    };

    this.isSubmitting.set(true);
    this.activitiesService
      .createActivity(payload)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSubmitting.set(false))
      )
      .subscribe({
        next: () => {
          this.toastr.success('Atividade registrada com sucesso.');
          this.resetForm();
          if (this.currentCourseId) {
            this.loadActivities(this.currentCourseId);
          }
        },
        error: () => this.toastr.error('Nao foi possivel registrar a atividade.')
      });
  }

  private onCourseChanged(course: CourseDto | null): void {
    const newId = course?.Id ?? null;
    this.prefillClassGroup(course);

    if (newId === this.currentCourseId) {
      return;
    }

    this.currentCourseId = newId;

    if (!newId) {
      this.activities.set([]);
      return;
    }

    this.loadActivities(newId);
  }

  private prefillClassGroup(course: CourseDto | null): void {
    const firstGroup = course ? (course.ClassGroups ?? []).find(group => !group.IsMaterialsDistribution) : null;
    const control = this.form.controls.classGroupId;
    const currentValue = control.value ?? '';

    if (!firstGroup) {
      control.setValue('', { emitEvent: false });
      return;
    }

    const stillValid = Boolean(course?.ClassGroups?.some(group => group.Id === currentValue));
    if (!currentValue || !stillValid) {
      control.setValue(firstGroup.Id, { emitEvent: false });
    }
  }

  private resetForm(): void {
    const defaultGroup = this.availableGroups()[0]?.Id ?? '';
    this.form.reset({
      classGroupId: defaultGroup,
      title: '',
      description: '',
      availableAt: null,
      dueDate: null,
      maxScore: null,
      allowLateSubmissions: false,
      latePenaltyPercentage: null,
      visibleToStudents: true
    });
    this.form.controls.latePenaltyPercentage.disable({ emitEvent: false });
    this.attachments.set([]);
  }

  private uploadFile(file: File): void {
    const draft: AttachmentDraft = {
      id: this.generateId(),
      fileName: file.name,
      status: 'uploading',
      caption: ''
    };
    this.attachments.update(list => [...list, draft]);

    this.mediaService
      .upload(file)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: media => {
          this.attachments.update(list =>
            list.map(item => (item.id === draft.id ? { ...item, media, status: 'ready' } : item))
          );
          this.toastr.success(`${file.name} enviado.`);
        },
        error: () => {
          this.attachments.update(list =>
            list.map(item => (item.id === draft.id ? { ...item, status: 'error' } : item))
          );
          this.toastr.error(`Falha ao enviar ${file.name}.`);
        }
      });
  }

  private loadActivities(courseId: string): void {
    this.activitiesLoading.set(true);
    this.activitiesService
      .getActivities({ CourseId: courseId })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: items => {
          this.activities.set(items);
          this.activitiesLoading.set(false);
        },
        error: () => {
          this.activitiesLoading.set(false);
          this.toastr.error('Nao foi possivel carregar as atividades.');
        }
      });
  }

  private syncLatePenaltyControl(enabled: boolean): void {
    const control = this.form.controls.latePenaltyPercentage;
    if (enabled) {
      control.enable({ emitEvent: false });
    } else {
      control.disable({ emitEvent: false });
      control.reset(null, { emitEvent: false });
    }
  }

  private toIsoString(value: string | null | undefined): string | undefined {
    if (!value) {
      return undefined;
    }
    const parsed = new Date(value);
    return Number.isNaN(parsed.getTime()) ? undefined : parsed.toISOString();
  }

  private parseDecimal(value: string | null | undefined): number | undefined {
    if (value === null || value === undefined || value === '') {
      return undefined;
    }
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : undefined;
  }

  private parseInteger(value: string | null | undefined): number | undefined {
    if (value === null || value === undefined || value === '') {
      return undefined;
    }
    const parsed = Number(value);
    return Number.isFinite(parsed) ? Math.max(0, Math.trunc(parsed)) : undefined;
  }

  private generateId(): string {
    if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
      return crypto.randomUUID();
    }
    return Math.random().toString(36).slice(2, 11);
  }
}
