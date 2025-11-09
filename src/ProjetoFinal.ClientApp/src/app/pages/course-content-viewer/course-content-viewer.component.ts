import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { switchMap } from 'rxjs/operators';

import { CourseContentsService } from '../../core/services/course-contents.service';
import { CourseContentDto, ContentAttachmentDto } from '../../core/api/contents.api';
import { MediaService } from '../../core/services/media.service';
import { ToastrService } from 'ngx-toastr';

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

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly content = signal<CourseContentDto | null>(null);
  readonly courseId = signal<string | null>(null);
  readonly downloading = signal<string | null>(null);

  readonly pageTitle = computed(() => this.content()?.Title ?? 'Conteudo');
  readonly statusLabel = computed(() => (this.content()?.IsDraft ? 'Rascunho' : 'Publicado'));

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
          this.loading.set(false);
          this.error.set(null);
        },
        error: () => {
          this.content.set(null);
          this.error.set('Nao foi possivel carregar o conteudo selecionado.');
          this.loading.set(false);
        }
      });
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
}
