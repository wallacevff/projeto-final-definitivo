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

import { CourseDto } from '../../core/api/courses.api';
import {
  ContentItemType,
  CourseContentCreatePayload,
  CourseContentListItem
} from '../../core/api/contents.api';
import { CourseContentsService } from '../../core/services/course-contents.service';
import { AuthService } from '../../core/services/auth.service';
import { MediaResource } from '../../core/api/media.api';
import { MediaService } from '../../core/services/media.service';

type AttachmentStatus = 'uploading' | 'ready' | 'error';

interface ContentAttachmentDraft {
  id: string;
  fileName: string;
  status: AttachmentStatus;
  caption: string;
  isPrimary: boolean;
  media?: MediaResource;
}

@Component({
  selector: 'app-course-contents',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './course-contents.component.html',
  styleUrl: './course-contents.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CourseContentsComponent {
  private readonly fb = inject(FormBuilder);
  private readonly contentsService = inject(CourseContentsService);
  private readonly mediaService = inject(MediaService);
  private readonly authService = inject(AuthService);
  private readonly toastr = inject(ToastrService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly courseState = signal<CourseDto | null>(null);
  private currentCourseId: string | null = null;

  readonly selectedCourse = computed(() => this.courseState());
  readonly contents = signal<CourseContentListItem[]>([]);
  readonly loading = signal(false);
  readonly formVisible = signal(false);
  readonly isSubmitting = signal(false);
  readonly attachments = signal<ContentAttachmentDraft[]>([]);
  readonly publishing = signal<string | null>(null);

  readonly itemTypeOptions: ReadonlyArray<{ value: ContentItemType; label: string }> = [
    { value: ContentItemType.Text, label: 'Texto' },
    { value: ContentItemType.File, label: 'Arquivo' },
    { value: ContentItemType.Video, label: 'Video' },
    { value: ContentItemType.Link, label: 'Link' }
  ];
  private readonly itemTypeLabels = {
    [ContentItemType.Text]: 'Texto',
    [ContentItemType.File]: 'Arquivo',
    [ContentItemType.Video]: 'Video',
    [ContentItemType.Link]: 'Link'
  } as const;

  readonly form = this.fb.group({
    classGroupId: this.fb.control<string | null>(null),
    title: this.fb.control('', { validators: [Validators.required, Validators.maxLength(180)] }),
    summary: this.fb.control('', { validators: [Validators.maxLength(400)] }),
    body: this.fb.control('', { validators: [Validators.maxLength(4000)] }),
    itemType: this.fb.control<ContentItemType>(ContentItemType.Text, { nonNullable: true }),
    displayOrder: this.fb.control<string | null>(null),
    publishNow: this.fb.control(true)
  });

  readonly hasUploadingAttachments = computed(() =>
    this.attachments().some(item => item.status === 'uploading')
  );

  readonly hasAvailableGroups = computed(() => (this.courseState()?.ClassGroups?.length ?? 0) > 0);

  @Input({ required: true })
  set course(value: CourseDto | null) {
    this.courseState.set(value);
    this.onCourseChanged(value);
  }

  toggleForm(): void {
    const open = !this.formVisible();
    this.formVisible.set(open);
    if (open) {
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
    const wasPrimary = this.attachments().find(item => item.id === id)?.isPrimary;
    this.attachments.update(list => list.filter(item => item.id !== id));
    if (wasPrimary) {
      this.ensurePrimaryAttachment();
    }
  }

  updateCaption(id: string, value: string): void {
    this.attachments.update(list =>
      list.map(item => (item.id === id ? { ...item, caption: value } : item))
    );
  }

  markAsPrimary(id: string): void {
    this.attachments.update(list =>
      list.map(item => ({
        ...item,
        isPrimary: item.id === id
      }))
    );
  }

  trackByContent(_: number, item: CourseContentListItem): string {
    return item.id;
  }

  trackByAttachment(_: number, item: ContentAttachmentDraft): string {
    return item.id;
  }

  itemTypeLabelFor(type: ContentItemType): string {
    return this.itemTypeLabels[type] ?? 'Conteudo';
  }

  submit(): void {
    const course = this.courseState();
    const currentUser = this.authService.currentUser();

    if (!course || !currentUser) {
      this.toastr.error('Usuario nao autenticado ou curso invalido.');
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (this.hasUploadingAttachments()) {
      this.toastr.info('Aguarde terminar o upload dos anexos.');
      return;
    }

    const payload = this.buildPayload(course.Id, currentUser.id);
    if (!payload) {
      return;
    }

    this.isSubmitting.set(true);
    this.contentsService
      .createContent(payload)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSubmitting.set(false))
      )
      .subscribe({
        next: () => {
          this.toastr.success('Conteudo registrado com sucesso.');
          this.resetForm();
          if (this.currentCourseId) {
            this.loadContents(this.currentCourseId);
          }
        },
        error: () => this.toastr.error('Nao foi possivel registrar o conteudo.')
      });
  }

  publishContent(contentId: string): void {
    this.publishing.set(contentId);
    this.contentsService
      .publishContent(contentId)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.publishing.set(null))
      )
      .subscribe({
        next: () => {
          this.toastr.success('Conteudo publicado.');
          if (this.currentCourseId) {
            this.loadContents(this.currentCourseId);
          }
        },
        error: () => this.toastr.error('Nao foi possivel publicar o conteudo.')
      });
  }

  private onCourseChanged(course: CourseDto | null): void {
    const courseId = course?.Id ?? null;
    if (courseId === this.currentCourseId) {
      return;
    }

    this.currentCourseId = courseId;
    if (!courseId) {
      this.contents.set([]);
      return;
    }

    this.loadContents(courseId);
  }

  private loadContents(courseId: string): void {
    this.loading.set(true);
    this.contentsService
      .getContents({ CourseId: courseId })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: items => {
          this.contents.set(items);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.toastr.error('Nao foi possivel carregar os conteudos.');
        }
      });
  }

  private uploadFile(file: File): void {
    const draft: ContentAttachmentDraft = {
      id: this.generateId(),
      fileName: file.name,
      status: 'uploading',
      caption: '',
      isPrimary: !this.attachments().some(item => item.isPrimary)
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
          this.ensurePrimaryAttachment();
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

  private buildPayload(courseId: string, authorId: string): CourseContentCreatePayload | null {
    const raw = this.form.getRawValue();
    const displayOrder = this.parseInteger(raw.displayOrder);
    if (displayOrder === null) {
      this.toastr.error('Informe uma ordem de exibicao valida (numero inteiro).');
      return null;
    }

    const title = (raw.title ?? '').trim();
    if (!title) {
      this.toastr.error('Informe um titulo para o conteudo.');
      return null;
    }

    const attachments = this.attachments()
      .filter(item => item.status === 'ready' && item.media)
      .map(item => ({
        MediaResourceId: item.media!.Id,
        Caption: item.caption?.trim() || undefined,
        IsPrimary: item.isPrimary
      }));

    if (attachments.length > 0 && !attachments.some(item => item.IsPrimary)) {
      attachments[0].IsPrimary = true;
    }

    const itemType = Number(raw.itemType ?? ContentItemType.Text) as ContentItemType;

    return {
      CourseId: courseId,
      ClassGroupId: raw.classGroupId || undefined,
      AuthorId: authorId,
      Title: title,
      Summary: raw.summary?.trim() || undefined,
      Body: raw.body?.trim() || undefined,
      ItemType: itemType,
      IsDraft: !raw.publishNow,
      DisplayOrder: displayOrder,
      Attachments: attachments
    };
  }

  private resetForm(): void {
    this.form.reset({
      classGroupId: null,
      title: '',
      summary: '',
      body: '',
      itemType: ContentItemType.Text,
      displayOrder: String((this.contents().length + 1) * 10),
      publishNow: true
    });
    this.attachments.set([]);
  }

  private ensurePrimaryAttachment(): void {
    const current = this.attachments();
    const hasPrimary = current.some(item => item.isPrimary && item.status === 'ready');
    if (hasPrimary || current.length === 0) {
      return;
    }

    const nextPrimaryId = current.find(item => item.status === 'ready')?.id ?? current[0].id;
    this.markAsPrimary(nextPrimaryId);
  }

  private parseInteger(value: string | null | undefined): number | null {
    if (value === null || value === undefined || value === '') {
      return 0;
    }
    const parsed = Number(value);
    if (!Number.isFinite(parsed)) {
      return null;
    }
    return Math.trunc(parsed);
  }

  private generateId(): string {
    if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
      return crypto.randomUUID();
    }
    return Math.random().toString(36).slice(2, 11);
  }
}
