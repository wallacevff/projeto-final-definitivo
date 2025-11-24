import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

import { ActivitiesService } from '../../core/services/activities.service';
import { ActivityAttachmentDto, ActivityDto } from '../../core/api/activities.api';
import { MediaKind, MediaResource } from '../../core/api/media.api';
import { MediaService } from '../../core/services/media.service';
import { RichTextEditorComponent } from '../../shared/components/rich-text-editor/rich-text-editor.component';
import { AuthService } from '../../core/services/auth.service';
import { ActivitySubmissionsService } from '../../core/services/activity-submissions.service';
import {
  ActivitySubmissionDto,
  SubmissionAttachmentDto
} from '../../core/api/activity-submissions.api';

interface SubmissionAttachmentDraft {
  id: string;
  fileName: string;
  status: 'uploading' | 'ready' | 'error';
  media?: MediaResource;
}

@Component({
  selector: 'app-course-activity-viewer',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, RichTextEditorComponent],
  templateUrl: './course-activity-viewer.component.html',
  styleUrl: './course-activity-viewer.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CourseActivityViewerComponent {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly activitiesService = inject(ActivitiesService);
  private readonly mediaService = inject(MediaService);
  private readonly toastr = inject(ToastrService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly authService = inject(AuthService);
  private readonly submissionsService = inject(ActivitySubmissionsService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly activity = signal<ActivityDto | null>(null);
  readonly courseId = signal<string | null>(null);
  readonly downloading = signal<string | null>(null);
  readonly existingSubmission = signal<ActivitySubmissionDto | null>(null);
  readonly submissionLoading = signal(false);
  readonly submissionError = signal<string | null>(null);
  readonly submissionAttachments = signal<SubmissionAttachmentDraft[]>([]);
  readonly isSubmittingWork = signal(false);

  readonly pageTitle = computed(() => this.activity()?.Title ?? 'Atividade');
  readonly dueDateLabel = computed(() => this.formatDate(this.activity()?.DueDate));
  readonly currentUser = this.authService.currentUser;
  readonly isStudent = computed(() => this.currentUser()?.role === 1);
  readonly isInstructor = computed(() => this.currentUser()?.role === 2);
  readonly hasUploadingSubmissionAttachments = computed(() =>
    this.submissionAttachments().some(item => item.status === 'uploading')
  );
  readonly submissionForm = this.fb.group({
    textAnswer: this.fb.control('', { validators: [Validators.maxLength(5000)] })
  });
  private readonly submissionStatusLabels: Record<number, string> = {
    1: 'Rascunho',
    2: 'Submetida',
    3: 'Corrigida',
    4: 'Devolvida'
  };

  constructor() {
    this.route.paramMap
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        switchMap(params => {
          const courseId = params.get('courseId');
          const activityId = params.get('activityId');
          this.courseId.set(courseId);
          if (!activityId) {
            throw new Error('Atividade nao encontrada.');
          }
          this.loading.set(true);
          return this.activitiesService.getActivityById(activityId);
        })
      )
      .subscribe({
        next: activity => {
          this.activity.set(activity);
          this.error.set(null);
          this.loading.set(false);
          this.resetSubmissionState();
          if (this.isStudent()) {
            this.fetchStudentSubmission(activity.Id);
          }
        },
        error: () => {
          this.activity.set(null);
          this.error.set('Nao foi possivel carregar a atividade selecionada.');
          this.loading.set(false);
        }
      });
  }

  readonly backLink = computed(() => {
    const id = this.courseId();
    if (this.isInstructor()) {
      return ['/courses', id ?? '', 'manage'];
    }
    return id ? ['/student/courses', id] : ['/dashboard'];
  });

  downloadAttachment(attachment: ActivityAttachmentDto): void {
    this.downloadMedia(attachment.MediaResourceId, attachment.Caption || 'anexo');
  }

  downloadSubmissionAttachment(attachment: SubmissionAttachmentDto): void {
    const fallback = attachment.Media?.OriginalFileName ?? 'Envio do aluno';
    this.downloadMedia(attachment.MediaResourceId, fallback);
  }

  handleSubmissionFiles(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = input.files;
    if (!files?.length) {
      return;
    }

    Array.from(files).forEach(file => this.uploadSubmissionFile(file));
    input.value = '';
  }

  removeSubmissionAttachment(id: string): void {
    this.submissionAttachments.update(list => list.filter(item => item.id !== id));
  }

  trackSubmissionAttachment(_: number, item: SubmissionAttachmentDraft): string {
    return item.id;
  }

  submitActivityWork(): void {
    const currentActivity = this.activity();
    const student = this.currentUser();
    if (!currentActivity || !student) {
      this.toastr.error('Atividade ou usuario invalido.');
      return;
    }

    if (this.existingSubmission()) {
      this.toastr.info('Voce ja enviou esta atividade.');
      return;
    }

    if (this.hasUploadingSubmissionAttachments()) {
      this.toastr.info('Aguarde o envio completo dos anexos.');
      return;
    }

    const textAnswer = (this.submissionForm.controls.textAnswer.value ?? '').trim();
    const readyAttachments = this.submissionAttachments()
      .filter(item => item.status === 'ready' && item.media)
      .map((item, index) => ({
        MediaResourceId: item.media!.Id,
        IsPrimary: index === 0,
        IsVideo: item.media?.Kind === MediaKind.Video
      }));

    if (!textAnswer && readyAttachments.length === 0) {
      this.toastr.warning('Adicione uma observacao ou pelo menos um anexo.');
      return;
    }

    const payload = {
      ActivityId: currentActivity.Id,
      StudentId: student.id,
      ClassGroupId: currentActivity.ClassGroupId,
      TextAnswer: textAnswer || undefined,
      Attachments: readyAttachments
    };

    this.isSubmittingWork.set(true);
    this.submissionsService
      .submit(payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: submission => {
          this.toastr.success('Atividade enviada com sucesso.');
          const submissionWithLocalAttachments = {
            ...submission,
            Attachments: this.buildUiAttachmentsFromDrafts()
          };
          this.existingSubmission.set(this.normalizeSubmission(submissionWithLocalAttachments));
          this.submissionAttachments.set([]);
          this.submissionForm.reset();
          this.isSubmittingWork.set(false);
          this.fetchStudentSubmission(currentActivity.Id);
        },
        error: () => {
          this.toastr.error('Nao foi possivel enviar sua atividade.');
          this.isSubmittingWork.set(false);
        }
      });
  }

  private formatDate(value?: string): string {
    if (!value) {
      return 'Sem data definida';
    }
    const parsed = new Date(value);
    if (Number.isNaN(parsed.getTime())) {
      return 'Sem data definida';
    }
    return parsed.toLocaleString('pt-BR', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }


  safeHtml(content?: string | null): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(content ?? '');
  }

  private downloadMedia(mediaResourceId: string, fileName: string): void {
    if (!mediaResourceId || this.downloading() === mediaResourceId) {
      return;
    }

    this.downloading.set(mediaResourceId);
    this.mediaService
      .download(mediaResourceId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: blob => {
          const url = URL.createObjectURL(blob);
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = fileName || 'anexo';
          anchor.click();
          URL.revokeObjectURL(url);
          this.downloading.set(null);
        },
        error: () => {
          this.toastr.error('Nao foi possivel baixar o anexo.');
          this.downloading.set(null);
        }
      });
  }

  private uploadSubmissionFile(file: File): void {
    const draft: SubmissionAttachmentDraft = {
      id: this.generateAttachmentId(),
      fileName: file.name,
      status: 'uploading'
    };

    this.submissionAttachments.update(list => [...list, draft]);
    this.mediaService
      .upload(file)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: media => {
          this.submissionAttachments.update(list =>
            list.map(item => (item.id === draft.id ? { ...item, status: 'ready', media } : item))
          );
        },
        error: () => {
          this.submissionAttachments.update(list =>
            list.map(item => (item.id === draft.id ? { ...item, status: 'error' } : item))
          );
          this.toastr.error(`Nao foi possivel enviar ${file.name}.`);
        }
      });
  }

  private fetchStudentSubmission(activityId: string): void {
    const student = this.currentUser();
    if (!student) {
      return;
    }

    this.submissionLoading.set(true);
    this.submissionError.set(null);
    this.submissionsService
      .getStudentSubmission(activityId, student.id)
      .pipe(
        switchMap(submission => {
          if (!submission) {
            return of(null);
          }
          return this.submissionsService.getById(submission.Id);
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: submission => {
          this.existingSubmission.set(this.normalizeSubmission(submission));
          this.submissionLoading.set(false);
        },
        error: () => {
          this.submissionError.set('Nao foi possivel verificar envios anteriores.');
          this.submissionLoading.set(false);
        }
      });
  }

  private resetSubmissionState(): void {
    this.submissionAttachments.set([]);
    this.submissionForm.reset();
    this.existingSubmission.set(null);
    this.submissionError.set(null);
    this.submissionLoading.set(false);
    this.isSubmittingWork.set(false);
  }

  private generateAttachmentId(): string {
    if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
      return crypto.randomUUID();
    }
    return Math.random().toString(36).slice(2, 11);
  }

  private normalizeSubmission(submission: ActivitySubmissionDto | null): ActivitySubmissionDto | null {
    if (!submission) {
      return null;
    }

    return {
      ...submission,
      Attachments: submission.Attachments ?? []
    };
  }

  submissionStatusLabel(status?: number): string {
    if (!status) {
      return '';
    }
    return this.submissionStatusLabels[status] ?? 'Desconhecido';
  }

  private buildUiAttachmentsFromDrafts(): SubmissionAttachmentDto[] {
    return this.submissionAttachments()
      .filter(item => item.status === 'ready' && item.media)
      .map((item, index) => ({
        Id: item.media?.Id ?? this.generateAttachmentId(),
        MediaResourceId: item.media!.Id,
        IsPrimary: index === 0,
        IsVideo: item.media?.Kind === MediaKind.Video,
        Media: item.media
      }));
  }
}
