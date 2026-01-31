import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ToastrService } from 'ngx-toastr';

import { CoursesService } from '../../core/services/courses.service';
import { CourseDto, CourseListItem, ClassGroupDto } from '../../core/api/courses.api';
import { AuthService } from '../../core/services/auth.service';
import { CourseSubscriptionsService } from '../../core/services/course-subscriptions.service';
import { ClassGroupsService } from '../../core/services/class-groups.service';

@Component({
  selector: 'app-courses',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './courses.component.html',
  styleUrl: './courses.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CoursesComponent {
  private readonly service = inject(CoursesService);
  private readonly authService = inject(AuthService);
  private readonly subscriptionsService = inject(CourseSubscriptionsService);
  private readonly classGroupsService = inject(ClassGroupsService);
  private readonly toastr = inject(ToastrService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly courses = signal<CourseListItem[]>([]);
  readonly titleFilter = signal('');
  readonly categoryFilter = signal('');
  readonly instructorFilter = signal('');

  readonly filteredCourses = computed(() => {
    const titleTerm = this.normalize(this.titleFilter());
    const categoryTerm = this.normalize(this.categoryFilter());
    const instructorTerm = this.normalize(this.instructorFilter());

    return this.courses().filter(course => {
      const matchesTitle = titleTerm
        ? this.normalize(course.title).includes(titleTerm)
        : true;
      const matchesCategory = categoryTerm
        ? this.normalize(course.category ?? '').includes(categoryTerm)
        : true;
      const matchesInstructor = instructorTerm
        ? this.normalize(course.instructor ?? '').includes(instructorTerm)
        : true;

      return matchesTitle && matchesCategory && matchesInstructor;
    });
  });

  readonly currentUser = this.authService.currentUser;
  readonly isStudentUser = computed(() => this.currentUser()?.role === 1);
  readonly isInstructorUser = computed(() => this.currentUser()?.role === 2);
  readonly subscribedCourseIds = signal<Set<string>>(new Set());
  readonly interactiveEnrollmentCourseIds = signal<Set<string>>(new Set());
  readonly enrollingCourseId = signal<string | null>(null);
  readonly enrollmentPanelLoading = signal(false);
  readonly enrollmentPanelError = signal<string | null>(null);
  readonly enrollmentPanelCourse = signal<CourseDto | null>(null);
  readonly selectedClassGroupId = signal<string | null>(null);
  readonly enrollmentCode = signal('');
  readonly enrollmentSubmitting = signal(false);
  readonly enrollmentCodeError = signal<string | null>(null);

  readonly interactiveGroups = computed(() => {
    const course = this.enrollmentPanelCourse();
    if (!course) {
      return [];
    }
    return (course.ClassGroups ?? []).filter(group => !group.IsMaterialsDistribution);
  });

  readonly requiresCodeForSelection = computed(() => {
    const selectedId = this.selectedClassGroupId();
    const groups = this.interactiveGroups();
    const selected = groups.find(group => group.Id === selectedId);
    return Boolean(selected?.RequiresEnrollmentCode);
  });

  readonly enrollmentActionLabel = computed(() => {
    const selectedId = this.selectedClassGroupId();
    const selected = this.interactiveGroups().find(group => group.Id === selectedId);
    if (!selected) {
      return 'Selecionar turma';
    }
    return selected.RequiresApproval ? 'Enviar solicitacao' : 'Entrar na turma';
  });

  constructor() {
    this.loadCourses();

    const student = this.currentUser();
    if (student?.id) {
      this.loadSubscriptions(student.id);
      this.loadInteractiveEnrollments(student.id);
    }
  }

  manageCourse(courseId: string): void {
    if (!this.isInstructorUser()) {
      return;
    }
    this.router.navigate(['/courses', courseId, 'manage']);
  }

  goToCreateCourse(): void {
    if (!this.isInstructorUser()) {
      return;
    }
    this.router.navigate(['/courses/create']);
  }

  trackByCourseId(_: number, item: CourseListItem): string {
    return item.id;
  }

  onTitleFilterChange(value: string): void {
    this.titleFilter.set(value);
  }

  onCategoryFilterChange(value: string): void {
    this.categoryFilter.set(value);
  }

  onInstructorFilterChange(value: string): void {
    this.instructorFilter.set(value);
  }

  resetFilters(): void {
    this.titleFilter.set('');
    this.categoryFilter.set('');
    this.instructorFilter.set('');
  }

  canSubscribe(course: CourseListItem): boolean {
    return this.isStudentUser() && course.mode === 2;
  }

  canRequestInteractiveEnrollment(course: CourseListItem): boolean {
    return this.isStudentUser() && course.mode === 1;
  }

  isInteractiveEnrolled(courseId: string): boolean {
    return this.interactiveEnrollmentCourseIds().has(courseId);
  }

  isSubscribed(courseId: string): boolean {
    return this.subscribedCourseIds().has(courseId);
  }

  isEnrolling(courseId: string): boolean {
    return this.enrollingCourseId() === courseId;
  }

  enrollInCourse(course: CourseListItem): void {
    if (!this.canSubscribe(course) || this.isSubscribed(course.id) || this.isEnrolling(course.id)) {
      return;
    }

    const student = this.currentUser();
    if (!student?.id) {
      this.toastr.error('Voce precisa estar autenticado como aluno para se inscrever.');
      return;
    }

    this.enrollingCourseId.set(course.id);

    this.subscriptionsService
      .subscribe({ CourseId: course.id, StudentId: student.id })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: subscription => {
          this.toastr.success('Inscricao realizada com sucesso.');
          this.enrollingCourseId.set(null);
          this.subscribedCourseIds.update(current => {
            const updated = new Set(current);
            updated.add(subscription.CourseId);
            return updated;
          });
        },
        error: () => {
          this.toastr.error('Nao foi possivel realizar a inscricao.');
          this.enrollingCourseId.set(null);
        }
      });
  }

  openInteractiveEnrollment(course: CourseListItem): void {
    if (!this.canRequestInteractiveEnrollment(course)) {
      this.toastr.error('Disponivel apenas para alunos.');
      return;
    }

    if (this.isInteractiveEnrolled(course.id)) {
      this.toastr.info('Voce ja possui uma solicitacao ou matricula ativa neste curso.');
      return;
    }

    this.enrollmentPanelLoading.set(true);
    this.enrollmentPanelError.set(null);
    this.enrollmentCode.set('');
    this.enrollmentCodeError.set(null);
    this.selectedClassGroupId.set(null);

    this.service
      .getCourseById(course.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: courseDetails => {
          const groups = (courseDetails.ClassGroups ?? []).filter(group => !group.IsMaterialsDistribution);
          if (!groups.length) {
            this.toastr.info('Este curso ainda nao possui turmas disponiveis.');
            this.enrollmentPanelLoading.set(false);
            return;
          }

          this.enrollmentPanelCourse.set(courseDetails);
          this.selectedClassGroupId.set(groups[0]?.Id ?? null);
          this.enrollmentPanelLoading.set(false);
        },
        error: () => {
          this.enrollmentPanelError.set('Nao foi possivel carregar as turmas.');
          this.enrollmentPanelLoading.set(false);
        }
      });
  }

  closeInteractiveEnrollment(): void {
    this.enrollmentPanelCourse.set(null);
    this.selectedClassGroupId.set(null);
    this.enrollmentCode.set('');
    this.enrollmentCodeError.set(null);
    this.enrollmentPanelError.set(null);
    this.enrollmentPanelLoading.set(false);
    this.enrollmentSubmitting.set(false);
  }

  selectClassGroup(groupId: string): void {
    this.selectedClassGroupId.set(groupId);
    this.enrollmentCode.set('');
    this.enrollmentCodeError.set(null);
  }

  onEnrollmentCodeChange(value: string): void {
    const nextValue = value ?? '';
    this.enrollmentCode.set(nextValue);
    if (nextValue.trim()) {
      this.enrollmentCodeError.set(null);
    }
  }

  submitClassGroupEnrollment(): void {
    const student = this.currentUser();
    const course = this.enrollmentPanelCourse();
    const classGroupId = this.selectedClassGroupId();
    if (!student?.id || !course || !classGroupId) {
      this.toastr.error('Selecione uma turma antes de prosseguir.');
      return;
    }

    const selectedGroup = this.interactiveGroups().find(group => group.Id === classGroupId);
    if (!selectedGroup) {
      this.toastr.error('Turma invalida.');
      return;
    }

    const codeValue = (this.enrollmentCode() ?? '').trim();
    if (selectedGroup.RequiresEnrollmentCode && !codeValue) {
      this.enrollmentCodeError.set('Informe o codigo de inscricao desta turma.');
      return;
    }

    const payload = {
      ClassGroupId: classGroupId,
      StudentId: student.id,
      EnrollmentCode: codeValue || undefined
    };

    this.enrollmentSubmitting.set(true);
    this.classGroupsService
      .requestEnrollment(classGroupId, payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: enrollment => {
          const message =
            enrollment.Status === 2
              ? 'Inscricao confirmada! Acesse suas aulas na area do aluno.'
              : 'Solicitacao enviada. Aguarde a aprovacao do instrutor.';
          this.toastr.success(message);
          this.enrollmentSubmitting.set(false);
          this.closeInteractiveEnrollment();
        },
        error: () => {
          this.toastr.error('Nao foi possivel enviar a solicitacao para esta turma.');
          this.enrollmentSubmitting.set(false);
        }
      });
  }

  trackByGroup(_: number, item: ClassGroupDto): string {
    return item.Id;
  }

  approvedEnrollments(course: CourseListItem): number {
    if (typeof course.approvedStudents === 'number') {
      return course.approvedStudents;
    }
    if (typeof course.pendingStudents === 'number') {
      return Math.max(0, course.enrolledStudents - course.pendingStudents);
    }
    return course.enrolledStudents;
  }

  percentOccupied(course: CourseListItem): number {
    if (!course.capacity) {
      return 0;
    }

    const approvedStudents = this.approvedEnrollments(course);
    return Math.min(100, Math.round((approvedStudents / course.capacity) * 100));
  }

  availableSlots(course: CourseListItem): number | null {
    if (!course.capacity) {
      return null;
    }

    return Math.max(0, course.capacity - course.enrolledStudents);
  }

  private loadCourses(): void {
    const user = this.currentUser();
    const filter = this.isInstructorUser() && user?.id
      ? { InstructorId: user.id }
      : { IsPublished: true };

    this.service
      .getCourseCards(filter)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (courses: CourseListItem[]) => {
          this.courses.set(courses);
          this.error.set(null);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Nao foi possivel carregar os cursos. Tente novamente mais tarde.');
          this.loading.set(false);
        }
      });
  }

  private loadSubscriptions(studentId: string): void {
    this.subscriptionsService
      .getByStudent(studentId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: subscriptions => {
          this.subscribedCourseIds.set(new Set(subscriptions.map(subscription => subscription.CourseId)));
        },
        error: () => {
          this.subscribedCourseIds.set(new Set());
        }
      });
  }

  private loadInteractiveEnrollments(studentId: string): void {
    this.service
      .getCoursesDto({ IsPublished: true })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: courses => {
          const enrolledIds = courses
            .filter(course => (course.ClassGroups ?? []).some(group =>
              (group.Enrollments ?? []).some(enrollment =>
                enrollment.StudentId === studentId && [1, 2].includes(enrollment.Status)
              )
            ))
            .map(course => course.Id);
          this.interactiveEnrollmentCourseIds.set(new Set(enrolledIds));
        },
        error: () => {
          this.interactiveEnrollmentCourseIds.set(new Set());
        }
      });
  }

  private normalize(value: string | null | undefined): string {
    if (!value) {
      return '';
    }

    return value
      .toLocaleLowerCase('pt-BR')
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .trim();
  }
}
