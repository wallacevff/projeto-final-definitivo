import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { forkJoin, of } from 'rxjs';
import { catchError, filter, map, switchMap } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

import { ActivitiesService } from '../../core/services/activities.service';
import { ActivityListItem } from '../../core/api/activities.api';
import { ActivitySubmissionsService } from '../../core/services/activity-submissions.service';
import { ActivitySubmissionDto } from '../../core/api/activity-submissions.api';
import { CoursesService } from '../../core/services/courses.service';
import { CourseDto } from '../../core/api/courses.api';
import { AuthService } from '../../core/services/auth.service';
import { MediaService } from '../../core/services/media.service';
import { UsersService } from '../../core/services/users.service';

interface CorrectionGroup {
  activityId: string;
  classGroupId: string;
  classGroupName: string;
}

@Component({
  selector: 'app-activity-corrections',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './activity-corrections.component.html',
  styleUrl: './activity-corrections.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ActivityCorrectionsComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);
  private readonly activitiesService = inject(ActivitiesService);
  private readonly submissionsService = inject(ActivitySubmissionsService);
  private readonly coursesService = inject(CoursesService);
  private readonly authService = inject(AuthService);
  private readonly mediaService = inject(MediaService);
  private readonly usersService = inject(UsersService);
  private readonly toastr = inject(ToastrService);
  private readonly sanitizer = inject(DomSanitizer);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly course = signal<CourseDto | null>(null);
  readonly activity = signal<ActivityListItem | null>(null);
  readonly correctionGroups = signal<CorrectionGroup[]>([]);
  readonly selectedGroup = signal<CorrectionGroup | null>(null);
  readonly submissionsLoading = signal(false);
  readonly submissionsError = signal<string | null>(null);
  readonly submissions = signal<ActivitySubmissionDto[]>([]);
  readonly selectedSubmission = signal<ActivitySubmissionDto | null>(null);
  readonly submissionDetailsLoading = signal(false);
  readonly downloadingAttachment = signal<string | null>(null);
  readonly studentNames = signal<Map<string, string>>(new Map());
  readonly videoUrls = signal<Record<string, string>>({});
  readonly videoLoading = signal<Record<string, boolean>>({});

  readonly submissionStatusOptions = [
    { value: 2, label: 'Submetida' },
    { value: 3, label: 'Corrigida' },
    { value: 4, label: 'Devolvida' }
  ];
  private readonly submissionStatusLabels: Record<number, string> = {
    1: 'Rascunho',
    2: 'Submetida',
    3: 'Corrigida',
    4: 'Devolvida'
  };
  private readonly videoExtensions = ['mp4', 'mkv', 'mpg', 'mpeg'];

  readonly gradingForm = this.fb.group({
    status: this.fb.control<number | null>(null, { validators: [Validators.required] }),
    score: this.fb.control<string | null>(null),
    feedback: this.fb.control<string | null>('')
  });

  readonly activityTitle = computed(() => this.activity()?.title ?? 'Atividade');

  constructor() {
    this.route.paramMap
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        map(params => params.get('activityId')),
        filter((activityId): activityId is string => Boolean(activityId)),
        switchMap(activityId =>
          this.activitiesService.getActivityById(activityId).pipe(
            switchMap(activityDto =>
              forkJoin({
                activityDto: of(activityDto),
                course: this.coursesService.getCourseById(activityDto.CourseId),
                activities: this.activitiesService.getActivities({ CourseId: activityDto.CourseId })
              })
            )
          )
        )
      )
      .subscribe({
        next: ({ activityDto, course, activities }) => {
          this.course.set(course);
          const current = activities.find(item => item.id === activityDto.Id);
          if (!current) {
            this.error.set('Atividade nao encontrada para correcoes.');
            this.loading.set(false);
            return;
          }

          this.activity.set(current);
          const groups = this.buildCorrectionGroups(current, activities);
          this.correctionGroups.set(groups);
          const selected = groups.find(group => group.activityId === current.id) ?? groups[0] ?? null;
          this.selectedGroup.set(selected);
          if (selected) {
            this.loadSubmissions(selected.activityId, selected.classGroupId);
          }
          this.loading.set(false);
          this.error.set(null);
        },
        error: () => {
          this.error.set('Nao foi possivel carregar as correcoes desta atividade.');
          this.loading.set(false);
        }
      });

    this.destroyRef.onDestroy(() => this.cleanupVideoUrls());
  }

  trackBySubmission(_: number, item: ActivitySubmissionDto): string {
    return item.Id;
  }

  trackByGroup(_: number, item: CorrectionGroup): string {
    return item.activityId;
  }

  backToActivities(): void {
    this.router.navigate(['/activities']);
  }

  selectGroup(activityId: string): void {
    const group = this.correctionGroups().find(item => item.activityId === activityId) ?? null;
    if (!group || this.selectedGroup()?.activityId === group.activityId) {
      return;
    }
    this.selectedGroup.set(group);
    this.selectedSubmission.set(null);
    this.resetVideoUrls();
    this.loadSubmissions(group.activityId, group.classGroupId);
  }

  selectSubmission(submissionId: string): void {
    if (!submissionId) {
      return;
    }
    this.resetVideoUrls();
    this.submissionDetailsLoading.set(true);
    this.gradingForm.reset({ status: null, score: null, feedback: '' });
    this.submissionsService
      .getById(submissionId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: submission => {
          this.selectedSubmission.set(submission);
          this.patchGradingForm(submission);
          if (submission.StudentName) {
            const updated = new Map(this.studentNames());
            updated.set(submission.StudentId, submission.StudentName);
            this.studentNames.set(updated);
          } else {
            this.ensureStudentNames([submission.StudentId]);
          }
          this.submissionDetailsLoading.set(false);
        },
        error: () => {
          this.submissionDetailsLoading.set(false);
          this.toastr.error('Nao foi possivel carregar os detalhes da submissao.');
        }
      });
  }

  saveGrading(): void {
    const submission = this.selectedSubmission();
    const instructor = this.authService.currentUser();

    if (!submission) {
      this.toastr.warning('Selecione uma submissao para corrigir.');
      return;
    }

    if (!instructor) {
      this.toastr.error('Usuario nao autenticado.');
      return;
    }

    if (this.gradingForm.invalid) {
      this.gradingForm.markAllAsTouched();
      return;
    }

    const parsedScore = this.parseScore(this.gradingForm.controls.score.value);
    if (parsedScore === null && this.gradingForm.controls.score.value) {
      this.toastr.error('Informe um valor numerico valido para a nota.');
      return;
    }

    const payload = {
      Status: this.gradingForm.controls.status.value ?? 3,
      Score: parsedScore ?? undefined,
      GradedById: instructor.id,
      Feedback: this.gradingForm.controls.feedback.value?.trim() || undefined,
      TextAnswer: submission.TextAnswer,
      Attachments: (submission.Attachments ?? []).map(attachment => ({
        MediaResourceId: attachment.MediaResourceId,
        IsPrimary: attachment.IsPrimary,
        IsVideo: attachment.IsVideo
      }))
    };

    this.submissionDetailsLoading.set(true);
    this.submissionsService
      .updateSubmission(submission.Id, payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: updated => {
          this.toastr.success('Correcao salva com sucesso.');
          this.selectedSubmission.set(updated);
          this.patchGradingForm(updated);
          this.submissionDetailsLoading.set(false);
          const group = this.selectedGroup();
          if (group) {
            this.loadSubmissions(group.activityId, group.classGroupId);
          }
        },
        error: () => {
          this.submissionDetailsLoading.set(false);
          this.toastr.error('Nao foi possivel salvar a correcao.');
        }
      });
  }

  submissionStatusLabel(status: number): string {
    return this.submissionStatusLabels[status] ?? 'Desconhecido';
  }

  submissionStudentName(submission: ActivitySubmissionDto): string {
    return this.studentNames().get(submission.StudentId) ?? submission.StudentName ?? 'Aluno';
  }

  downloadSubmissionAttachment(attachmentId: string, fileName?: string): void {
    if (!attachmentId || this.downloadingAttachment() === attachmentId) {
      return;
    }

    this.downloadingAttachment.set(attachmentId);
    this.mediaService
      .download(attachmentId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: blob => {
          const url = URL.createObjectURL(blob);
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = fileName || 'anexo';
          anchor.click();
          URL.revokeObjectURL(url);
          this.downloadingAttachment.set(null);
        },
        error: () => {
          this.downloadingAttachment.set(null);
          this.toastr.error('Nao foi possivel baixar o anexo.');
        }
      });
  }

  isVideoAttachment(attachment: ActivitySubmissionDto['Attachments'][number]): boolean {
    if (attachment.IsVideo) {
      return true;
    }

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

  videoIsLoading(attachmentId: string): boolean {
    return Boolean(this.videoLoading()[attachmentId]);
  }

  loadVideo(attachment: ActivitySubmissionDto['Attachments'][number]): void {
    if (!attachment?.MediaResourceId || this.videoIsLoading(attachment.MediaResourceId)) {
      return;
    }

    if (this.videoUrlFor(attachment.MediaResourceId)) {
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
          this.videoLoading.update(state => ({ ...state, [attachment.MediaResourceId]: false }));
          this.toastr.error('Nao foi possivel carregar o video.');
        }
      });
  }

  safeHtml(content?: string | null): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(content ?? '');
  }

  private loadSubmissions(activityId: string, classGroupId: string): void {
    this.submissionsLoading.set(true);
    this.submissionsError.set(null);
    this.submissionsService
      .getSubmissions({ ActivityId: activityId, ClassGroupId: classGroupId, PageSize: 100 })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: response => {
          this.submissions.set(response.items);
          this.ensureStudentNames(response.items.map(item => item.StudentId));
          this.submissionsLoading.set(false);
          if (response.items.length) {
            this.selectSubmission(response.items[0].Id);
          } else {
            this.selectedSubmission.set(null);
          }
        },
        error: () => {
          this.submissionsLoading.set(false);
          this.submissionsError.set('Nao foi possivel carregar as submissões desta atividade.');
          this.submissions.set([]);
          this.selectedSubmission.set(null);
        }
      });
  }

  private patchGradingForm(submission: ActivitySubmissionDto): void {
    this.gradingForm.patchValue({
      status: submission.Status ?? 2,
      score: submission.Score != null ? String(submission.Score) : null,
      feedback: submission.Feedback ?? ''
    });
  }

  private parseScore(value: string | null): number | null {
    if (!value) {
      return null;
    }
    const parsed = Number(value);
    if (Number.isNaN(parsed)) {
      return null;
    }
    return parsed;
  }

  private ensureStudentNames(studentIds: string[]): void {
    const cache = this.studentNames();
    const pending = Array.from(new Set(studentIds.filter(id => !!id && !cache.has(id))));
    if (!pending.length) {
      return;
    }

    forkJoin(
      pending.map(id =>
        this.usersService.getById(id).pipe(catchError(() => of(null)))
      )
    )
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(users => {
        const updated = new Map(this.studentNames());
        users.forEach(user => {
          if (user) {
            updated.set(user.Id, user.FullName || user.Email || 'Aluno');
          }
        });
        this.studentNames.set(updated);
      });
  }

  private buildCorrectionGroups(current: ActivityListItem, activities: ActivityListItem[]): CorrectionGroup[] {
    const signature = this.activitySignature(current);
    const groups = activities
      .filter(item => this.activitySignature(item) === signature)
      .map(item => ({
        activityId: item.id,
        classGroupId: item.classGroupId,
        classGroupName: item.classGroupName || 'Turma nao informada'
      }));

    if (groups.length) {
      return groups;
    }

    return [
      {
        activityId: current.id,
        classGroupId: current.classGroupId,
        classGroupName: current.classGroupName || 'Turma nao informada'
      }
    ];
  }

  private activitySignature(activity: ActivityListItem): string {
    return [
      activity.courseId,
      activity.title,
      activity.dueDateLabel,
      String(activity.allowLate),
      String(activity.attachments)
    ].join('|');
  }

  private resetVideoUrls(): void {
    this.cleanupVideoUrls();
    this.videoUrls.set({});
    this.videoLoading.set({});
  }

  private cleanupVideoUrls(): void {
    const urls = Object.values(this.videoUrls());
    urls.forEach(url => URL.revokeObjectURL(url));
  }
}
