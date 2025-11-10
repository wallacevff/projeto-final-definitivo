import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { switchMap } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

import { ActivitiesService } from '../../core/services/activities.service';
import { ActivityAttachmentDto, ActivityDto } from '../../core/api/activities.api';
import { MediaService } from '../../core/services/media.service';

@Component({
  selector: 'app-course-activity-viewer',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './course-activity-viewer.component.html',
  styleUrl: './course-activity-viewer.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CourseActivityViewerComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly activitiesService = inject(ActivitiesService);
  private readonly mediaService = inject(MediaService);
  private readonly toastr = inject(ToastrService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly sanitizer = inject(DomSanitizer);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly activity = signal<ActivityDto | null>(null);
  readonly courseId = signal<string | null>(null);
  readonly downloading = signal<string | null>(null);

  readonly pageTitle = computed(() => this.activity()?.Title ?? 'Atividade');
  readonly dueDateLabel = computed(() => this.formatDate(this.activity()?.DueDate));

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
        },
        error: () => {
          this.activity.set(null);
          this.error.set('Nao foi possivel carregar a atividade selecionada.');
          this.loading.set(false);
        }
      });
  }

  downloadAttachment(attachment: ActivityAttachmentDto): void {
    if (!attachment.MediaResourceId || this.downloading() === attachment.MediaResourceId) {
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
          anchor.download = attachment.Caption || 'anexo';
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
}
