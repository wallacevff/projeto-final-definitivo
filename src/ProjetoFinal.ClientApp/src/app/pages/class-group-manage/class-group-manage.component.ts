import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ToastrService } from 'ngx-toastr';
import { forkJoin, of } from 'rxjs';
import { catchError, filter, map, switchMap } from 'rxjs/operators';

import { ClassGroupsService } from '../../core/services/class-groups.service';
import { CoursesService } from '../../core/services/courses.service';
import { ForumService } from '../../core/services/forum.service';
import { AuthService } from '../../core/services/auth.service';
import { ClassGroupDto, ClassEnrollmentDto, CourseDto } from '../../core/api/courses.api';
import { ForumThreadCreatePayload, ForumThreadListItem } from '../../core/api/forum.api';
import { ActivitiesService } from '../../core/services/activities.service';
import { ActivityListItem } from '../../core/api/activities.api';
import { ActivitySubmissionsService } from '../../core/services/activity-submissions.service';
import { ActivitySubmissionDto } from '../../core/api/activity-submissions.api';
import { MediaService } from '../../core/services/media.service';
import { UsersService } from '../../core/services/users.service';

@Component({
  selector: 'app-class-group-manage',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './class-group-manage.component.html',
  styleUrl: './class-group-manage.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClassGroupManageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);
  private readonly classGroupsService = inject(ClassGroupsService);
  private readonly coursesService = inject(CoursesService);
  private readonly forumService = inject(ForumService);
  private readonly authService = inject(AuthService);
  private readonly toastr = inject(ToastrService);
  private readonly activitiesService = inject(ActivitiesService);
  private readonly submissionsService = inject(ActivitySubmissionsService);
  private readonly mediaService = inject(MediaService);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly usersService = inject(UsersService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly threadsLoading = signal(false);
  readonly classGroup = signal<ClassGroupDto | null>(null);
  readonly course = signal<CourseDto | null>(null);
  readonly threads = signal<ForumThreadListItem[]>([]);
  readonly isThreadFormVisible = signal(false);
  readonly isThreadSubmitting = signal(false);
  readonly activitiesLoading = signal(false);
  readonly activities = signal<ActivityListItem[]>([]);
  readonly selectedActivityId = signal<string | null>(null);
  readonly submissionsLoading = signal(false);
  readonly submissionsError = signal<string | null>(null);
  readonly submissions = signal<ActivitySubmissionDto[]>([]);
  readonly selectedSubmission = signal<ActivitySubmissionDto | null>(null);
  readonly submissionDetailsLoading = signal(false);
  readonly downloadingAttachment = signal<string | null>(null);
  readonly studentNames = signal<Map<string, string>>(new Map());
  readonly videoUrls = signal<Record<string, string>>({});
  readonly videoLoading = signal<Record<string, boolean>>({});

  readonly capacity = computed(() => this.classGroup()?.Capacity ?? 0);
  readonly approved = computed(() => this.classGroup()?.ApprovedEnrollments ?? 0);
  readonly pending = computed(() => this.classGroup()?.PendingEnrollments ?? 0);
  readonly available = computed(() => {
    const cap = this.capacity();
    const approved = this.approved();
    return Math.max(cap - approved, 0);
  });
  readonly occupancyPercent = computed(() => {
    const cap = this.capacity();
    const approved = this.approved();
    if (!cap) {
      return 0;
    }
    return Math.min(100, Math.round((approved / cap) * 100));
  });
  readonly enrollments = computed<ClassEnrollmentDto[]>(() => this.classGroup()?.Enrollments ?? []);
  readonly selectedActivity = computed(() => {
    const activityId = this.selectedActivityId();
    return this.activities().find(activity => activity.id === activityId) ?? null;
  });

  readonly threadForm = this.fb.group({
    title: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(180)]),
    description: this.fb.control<string | null>('', [Validators.maxLength(1200)]),
    isPinned: this.fb.nonNullable.control(false)
  });
  readonly gradingForm = this.fb.group({
    status: this.fb.control<number | null>(null, { validators: [Validators.required] }),
    score: this.fb.control<string | null>(null),
    feedback: this.fb.control<string | null>(''),
  });

  private readonly enrollmentStatusLabels: Record<number, string> = {
    1: 'Pendente',
    2: 'Aprovada',
    3: 'Recusada',
    4: 'Cancelada'
  };
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

  constructor() {
    this.route.paramMap
      .pipe(
        map(params => params.get('classGroupId')),
        filter((classGroupId): classGroupId is string => Boolean(classGroupId)),
        switchMap(classGroupId =>
          this.classGroupsService.getClassGroupById(classGroupId).pipe(
            switchMap(group =>
              forkJoin({
                group: of(group),
                course: this.coursesService.getCourseById(group.CourseId)
              })
            ),
            map(result => ({ ...result, classGroupId }))
          )
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: ({ classGroupId, group, course }) => {
          this.classGroup.set(group);
          this.course.set(course);
          this.hydrateStudentNamesFromEnrollments(group.Enrollments ?? []);
          this.error.set(null);
          this.loading.set(false);
          this.reloadThreads();
          this.loadActivities(group.Id);
        },
        error: () => {
          this.classGroup.set(null);
          this.course.set(null);
          this.error.set('Nao foi possivel carregar os dados da turma.');
          this.loading.set(false);
        }
      });

    this.destroyRef.onDestroy(() => this.cleanupVideoUrls());
  }

  openThreadForm(): void {
    this.isThreadFormVisible.set(true);
    this.threadForm.reset({ title: '', description: '', isPinned: false });
  }

  cancelThreadCreation(): void {
    this.threadForm.reset({ title: '', description: '', isPinned: false });
    this.isThreadFormVisible.set(false);
  }

  statusLabel(status: number): string {
    return this.enrollmentStatusLabels[status] ?? 'Desconhecido';
  }

  statusBadgeClass(status: number): string {
    switch (status) {
      case 1:
        return 'badge--warning';
      case 2:
        return 'badge--success';
      case 3:
        return 'badge--danger';
      case 4:
        return 'badge--neutral';
      default:
        return 'badge--neutral';
    }
  }

  submitThread(): void {
    if (this.threadForm.invalid) {
      this.threadForm.markAllAsTouched();
      return;
    }

    const group = this.classGroup();
    const currentUser = this.authService.currentUser();

    if (!group) {
      this.toastr.error('Carregue os dados da turma antes de criar um topico.');
      return;
    }

    if (!currentUser) {
      this.toastr.error('Usuario nao autenticado.');
      return;
    }

    const raw = this.threadForm.getRawValue();
    const title = raw.title.trim();
    if (!title) {
      this.threadForm.controls.title.setErrors({ required: true });
      this.threadForm.controls.title.markAsTouched();
      return;
    }

    const payload: ForumThreadCreatePayload = {
      ClassGroupId: group.Id,
      CreatedById: currentUser.id,
      Title: title,
      Description: raw.description?.trim() || undefined,
      IsPinned: raw.isPinned
    };

    this.isThreadSubmitting.set(true);

    this.forumService
      .createThread(payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastr.success('Topico criado com sucesso.');
          this.isThreadSubmitting.set(false);
          this.isThreadFormVisible.set(false);
          this.threadForm.reset({ title: '', description: '', isPinned: false });
          this.reloadThreads();
        },
        error: () => {
          this.toastr.error('Nao foi possivel criar o topico.');
          this.isThreadSubmitting.set(false);
        }
      });
  }

  navigateBack(): void {
    this.router.navigate(['/class-groups']);
  }

  private reloadThreads(): void {
    const group = this.classGroup();
    const course = this.course();

    if (!group || !course) {
      return;
    }

    const lookup = new Map<string, string>([[course.Id, course.Title]]);
    this.threadsLoading.set(true);

    this.forumService
      .getThreadsByClassGroup(lookup, group.Id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: threads => {
          this.threads.set(threads);
          this.threadsLoading.set(false);
        },
        error: () => {
          this.toastr.error('Nao foi possivel carregar os topicos da turma.');
          this.threads.set([]);
          this.threadsLoading.set(false);
        }
      });
  }

  trackByThread(_: number, item: ForumThreadListItem): string {
    return item.id;
  }

  trackByActivity(_: number, item: ActivityListItem): string {
    return item.id;
  }

  trackBySubmission(_: number, item: ActivitySubmissionDto): string {
    return item.Id;
  }

  private loadActivities(classGroupId: string): void {
    this.activitiesLoading.set(true);
    this.activitiesService
      .getActivities({ ClassGroupId: classGroupId })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: items => {
          this.activities.set(items);
          this.activitiesLoading.set(false);
          if (items.length) {
            this.selectActivity(items[0].id);
          } else {
            this.selectedActivityId.set(null);
            this.submissions.set([]);
            this.selectedSubmission.set(null);
          }
        },
        error: () => {
          this.activitiesLoading.set(false);
          this.toastr.error('Nao foi possivel carregar as atividades desta turma.');
        }
      });
  }

  selectActivity(activityId: string): void {
    if (this.selectedActivityId() === activityId) {
      return;
    }
    this.selectedActivityId.set(activityId);
    this.selectedSubmission.set(null);
    const group = this.classGroup();
    if (!group) {
      return;
    }
    this.loadSubmissions(activityId, group.Id);
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

  private patchGradingForm(submission: ActivitySubmissionDto): void {
    this.gradingForm.patchValue({
      status: submission.Status ?? 2,
      score: submission.Score != null ? String(submission.Score) : null,
      feedback: submission.Feedback ?? ''
    });
  }

  submissionStatusLabel(status: number): string {
    return this.submissionStatusLabels[status] ?? 'Desconhecido';
  }

  safeHtml(content?: string | null): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(content ?? '');
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
          const activityId = this.selectedActivityId();
          const group = this.classGroup();
          if (activityId && group) {
            this.loadSubmissions(activityId, group.Id);
          }
        },
        error: () => {
          this.submissionDetailsLoading.set(false);
          this.toastr.error('Nao foi possivel salvar a correcao.');
        }
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

  submissionStudentName(submission: ActivitySubmissionDto): string {
    return this.studentNames().get(submission.StudentId) ?? submission.StudentName ?? 'Aluno';
  }

  private hydrateStudentNamesFromEnrollments(enrollments: ClassEnrollmentDto[]): void {
    if (!enrollments?.length) {
      return;
    }
    const current = new Map(this.studentNames());
    enrollments.forEach(enrollment => {
      current.set(enrollment.StudentId, enrollment.StudentName);
    });
    this.studentNames.set(current);
  }

  private ensureStudentNames(studentIds: string[]): void {
    const cache = this.studentNames();
    const pending = Array.from(new Set(studentIds.filter(id => !!id && !cache.has(id))));
    if (!pending.length) {
      return;
    }

    forkJoin(
      pending.map(id =>
        this.usersService.getById(id).pipe(
          catchError(() => of(null))
        )
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
