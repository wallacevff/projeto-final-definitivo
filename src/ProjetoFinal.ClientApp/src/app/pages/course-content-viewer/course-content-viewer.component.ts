import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { switchMap } from 'rxjs/operators';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

import { CourseContentsService } from '../../core/services/course-contents.service';
import { CourseContentDto, ContentAttachmentDto } from '../../core/api/contents.api';
import { MediaService } from '../../core/services/media.service';
import { ToastrService } from 'ngx-toastr';
import { ContentAnnotationsService } from '../../core/services/content-annotations.service';

interface LocalVideoAnnotation {
  id: string;
  time: number;
  text: string;
}

@Component({
  selector: 'app-course-content-viewer',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './course-content-viewer.component.html',
  styleUrl: './course-content-viewer.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CourseContentViewerComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly service = inject(CourseContentsService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly mediaService = inject(MediaService);
  private readonly toastr = inject(ToastrService);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly annotationsService = inject(ContentAnnotationsService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly content = signal<CourseContentDto | null>(null);
  readonly courseId = signal<string | null>(null);
  readonly downloading = signal<string | null>(null);
  readonly videoUrls = signal<Record<string, string>>({});
  readonly videoLoading = signal<Record<string, boolean>>({});
  readonly annotationDrafts = signal<Record<string, string>>({});
  readonly annotationsState = signal<Record<string, LocalVideoAnnotation[]>>({});

  readonly pageTitle = computed(() => this.content()?.Title ?? 'Conteudo');
  readonly statusLabel = computed(() => (this.content()?.IsDraft ? 'Rascunho' : 'Publicado'));
  private readonly videoExtensions = ['mp4', 'mkv', 'mpg', 'mpeg'];

  constructor() {
    this.route.paramMap
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        switchMap(params => {
          const courseId = params.get('courseId');
          const contentId = params.get('contentId');
          this.courseId.set(courseId);
          if (!contentId) {
            throw new Error('Conteudo nao encontrado.');
          }
          this.loading.set(true);
          return this.service.getContentById(contentId);
        })
      )
      .subscribe({
        next: content => {
          this.content.set(content);
          this.loadPersistedAnnotations(content.Attachments ?? []);
          this.loading.set(false);
          this.error.set(null);
        },
        error: () => {
          this.content.set(null);
          this.error.set('Nao foi possivel carregar o conteudo selecionado.');
          this.loading.set(false);
        }
      });

    this.destroyRef.onDestroy(() => this.cleanupVideoUrls());
  }

  downloadAttachment(attachment: ContentAttachmentDto): void {
    if (!attachment?.MediaResourceId) {
      return;
    }

    this.downloading.set(attachment.MediaResourceId);
    this.mediaService
      .download(attachment.MediaResourceId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: blob => {
          const url = URL.createObjectURL(blob);
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = attachment.Media?.OriginalFileName || attachment.Media?.FileName || 'anexo';
          anchor.click();
          URL.revokeObjectURL(url);
          this.downloading.set(null);
        },
        error: () => {
          this.toastr.error('Nao foi possivel baixar o arquivo.');
          this.downloading.set(null);
        }
      });
  }

  isVideoAttachment(attachment: ContentAttachmentDto): boolean {
    const contentType = attachment.Media?.ContentType?.toLowerCase() ?? '';
    if (contentType.startsWith('video/')) {
      return true;
    }

    const fileName = (attachment.Media?.OriginalFileName ?? attachment.Media?.FileName ?? '').toLowerCase();
    const extension = fileName.split('.').pop() ?? '';
    return this.videoExtensions.includes(extension);
  }

  videoUrlFor(attachmentId: string): string | null {
    return this.videoUrls()[attachmentId] ?? null;
  }

  annotationDraft(attachmentId: string): string {
    return this.annotationDrafts()[attachmentId] ?? '';
  }

  annotationsFor(attachmentId: string): LocalVideoAnnotation[] {
    return this.annotationsState()[attachmentId] ?? [];
  }

  loadVideo(attachment: ContentAttachmentDto): void {
    if (!attachment.MediaResourceId || this.videoLoading()[attachment.MediaResourceId]) {
      return;
    }

    this.videoLoading.update(state => ({ ...state, [attachment.MediaResourceId]: true }));

    this.mediaService
      .download(attachment.MediaResourceId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: blob => {
          const url = URL.createObjectURL(blob);
          this.videoUrls.update(state => ({ ...state, [attachment.MediaResourceId]: url }));
          this.videoLoading.update(state => ({ ...state, [attachment.MediaResourceId]: false }));
        },
        error: () => {
          this.toastr.error('Nao foi possivel carregar o video.');
          this.videoLoading.update(state => ({ ...state, [attachment.MediaResourceId]: false }));
        }
      });
  }

  updateAnnotationDraft(attachmentId: string, value: string): void {
    this.annotationDrafts.update(state => ({ ...state, [attachmentId]: value }));
  }

  addAnnotation(attachmentId: string, currentTime: number): void {
    const text = (this.annotationDrafts()[attachmentId] ?? '').trim();
    if (!text) {
      this.toastr.info('Digite uma anotacao antes de salvar.');
      return;
    }

    this.annotationsService
      .addAnnotation({
        ContentAttachmentId: attachmentId,
        TimeMarkerSeconds: currentTime,
        Comment: text
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: saved => {
          const annotation: LocalVideoAnnotation = {
            id: saved.Id,
            time: saved.TimeMarkerSeconds,
            text: saved.Comment
          };
          this.annotationsState.update(state => {
            const list = state[attachmentId] ?? [];
            return { ...state, [attachmentId]: [...list, annotation].sort((a, b) => a.time - b.time) };
          });
          this.annotationDrafts.update(state => ({ ...state, [attachmentId]: '' }));
        },
        error: () => {
          this.toastr.error('Nao foi possivel salvar a anotacao.');
        }
      });
  }

  seekTo(video: HTMLVideoElement, time: number): void {
    if (Number.isFinite(time) && video) {
      video.currentTime = time;
      video.focus();
    }
  }

  formatTime(time: number): string {
    if (!Number.isFinite(time)) {
      return '00:00';
    }
    const minutes = Math.floor(time / 60)
      .toString()
      .padStart(2, '0');
    const seconds = Math.floor(time % 60)
      .toString()
      .padStart(2, '0');
    return `${minutes}:${seconds}`;
  }

  isVideoLoaded(attachmentId: string): boolean {
    return Boolean(this.videoUrls()[attachmentId]);
  }

  videoIsLoading(attachmentId: string): boolean {
    return Boolean(this.videoLoading()[attachmentId]);
  }

  private cleanupVideoUrls(): void {
    const urls = Object.values(this.videoUrls());
    urls.forEach(url => URL.revokeObjectURL(url));
  }

  private loadPersistedAnnotations(attachments: ContentAttachmentDto[]): void {
    attachments
      .filter(attachment => this.isVideoAttachment(attachment))
      .forEach(attachment => {
        this.annotationsService
          .getAnnotations({ ContentAttachmentId: attachment.Id })
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: response => {
              const annotations = response.items
                .map(item => ({
                  id: item.Id,
                  time: item.TimeMarkerSeconds,
                  text: item.Comment
                }))
                .sort((a, b) => a.time - b.time);
              this.annotationsState.update(state => ({ ...state, [attachment.Id]: annotations }));
            },
            error: () => {
              this.toastr.error('Nao foi possivel carregar as anotacoes do video.');
            }
          });
      });
  }

  safeHtml(content?: string | null): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(content ?? '');
  }
}
