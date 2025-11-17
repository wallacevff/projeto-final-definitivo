import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin } from 'rxjs';

import { CourseDto, CourseListItem } from '../../core/api/courses.api';
import { AuthService } from '../../core/services/auth.service';
import { CourseSubscriptionsService } from '../../core/services/course-subscriptions.service';
import { CoursesService } from '../../core/services/courses.service';

interface HighlightCard {
  label: string;
  value: string;
  trend: 'up' | 'down' | 'steady';
  context: string;
}

interface TimelineItem {
  title: string;
  date: string;
  description: string;
}

interface StudentCourseCard extends CourseListItem {
  subscribedAt?: string;
  enrollmentType: 'distribution' | 'interactive';
  classGroupName?: string;
  enrollmentStatus?: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent {
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly coursesService = inject(CoursesService);
  private readonly subscriptionsService = inject(CourseSubscriptionsService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly modeLabels: Record<number, string> = {
    1: 'Interactivo',
    2: 'Assincrono'
  };
  private readonly enrollmentStatusLabels: Record<number, string> = {
    1: 'Pendente',
    2: 'Aprovada',
    3: 'Recusada',
    4: 'Cancelada'
  };

  readonly currentUser = this.authService.currentUser;
  readonly isStudentView = computed(() => this.currentUser()?.role === 1);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly studentCourses = signal<StudentCourseCard[]>([]);

  readonly highlightCards: HighlightCard[] = [
    { label: 'Alunos ativos', value: '1.287', trend: 'up', context: '+6,4% vs. semana anterior' },
    { label: 'Taxa de conclusão', value: '78%', trend: 'steady', context: 'Meta trimestral: 80%' },
    { label: 'Atividades pendentes', value: '54', trend: 'down', context: '-12 desde ontem' },
  ];

  readonly timeline: TimelineItem[] = [
    {
      title: 'Lançamento do curso de Transformação Digital',
      date: '02/out às 09:00',
      description: 'Configurar materiais e liberar matrículas para a turma piloto.'
    },
    {
      title: 'Reunião com instrutores',
      date: '03/out às 14:30',
      description: 'Alinhamento sobre trilhas de aprendizado e feedback dos alunos.'
    },
    {
      title: 'Prazo para avaliação de atividades',
      date: '05/out',
      description: 'Concluir correção das submissões pendentes de Design Thinking.'
    },
  ];

  constructor() {
    if (!this.isStudentView()) {
      this.loading.set(false);
      return;
    }

    const studentId = this.currentUser()?.id;
    if (!studentId) {
      this.error.set('Usuario nao identificado.');
      this.loading.set(false);
      return;
    }

    this.loadStudentCourses(studentId);
  }

  goToCreateCourse(): void {
    this.router.navigate(['/courses/create']);
  }

  trackByCourse(_: number, item: StudentCourseCard): string {
    return item.id;
  }

  enrollmentStatusLabel(status?: number): string {
    return status ? this.enrollmentStatusLabels[status] ?? 'Desconhecido' : '';
  }

  enrollmentStatusClass(status?: number): string {
    switch (status) {
      case 2:
        return 'status-badge status-badge--success';
      case 1:
        return 'status-badge status-badge--warning';
      case 3:
        return 'status-badge status-badge--danger';
      case 4:
        return 'status-badge status-badge--neutral';
      default:
        return 'status-badge status-badge--neutral';
    }
  }

  canAccessCourse(course: StudentCourseCard): boolean {
    if (course.enrollmentType === 'distribution') {
      return true;
    }
    return course.enrollmentStatus === 2;
  }

  openCourse(courseId: string): void {
    this.router.navigate(['/student/courses', courseId]);
  }

  private loadStudentCourses(studentId: string): void {
    forkJoin({
      subscriptions: this.subscriptionsService.getByStudent(studentId),
      courseCards: this.coursesService.getCourseCards({ IsPublished: true }),
      courseDetails: this.coursesService.getCoursesDto({ IsPublished: true })
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ subscriptions, courseCards, courseDetails }) => {
          const cardsById = new Map(courseCards.map(course => [course.id, course]));
          const courseDtoById = new Map(courseDetails.map(course => [course.Id, course]));
          const mergedCourses: StudentCourseCard[] = [];
          const addedCourseIds = new Set<string>();

          subscriptions.forEach(subscription => {
            const courseCard =
              cardsById.get(subscription.CourseId) ?? this.mapCourseDtoToListItem(courseDtoById.get(subscription.CourseId));
            if (!courseCard) {
              return;
            }
            mergedCourses.push({
              ...courseCard,
              subscribedAt: subscription.SubscribedAt,
              enrollmentType: 'distribution'
            });
            addedCourseIds.add(courseCard.id);
          });

          courseDetails.forEach(course => {
            if (addedCourseIds.has(course.Id)) {
              return;
            }
            const enrollment = this.resolveStudentEnrollment(course, studentId);
            if (!enrollment) {
              return;
            }
            const courseCard = cardsById.get(course.Id) ?? this.mapCourseDtoToListItem(course);
            if (!courseCard) {
              return;
            }
            mergedCourses.push({
              ...courseCard,
              subscribedAt: enrollment.date,
              enrollmentType: 'interactive',
              classGroupName: enrollment.groupName,
              enrollmentStatus: enrollment.status
            });
            addedCourseIds.add(course.Id);
          });

          this.studentCourses.set(mergedCourses);
          this.error.set(null);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Nao foi possivel carregar seus cursos inscritos.');
          this.loading.set(false);
        }
      });
  }

  private mapCourseDtoToListItem(course?: CourseDto): CourseListItem | null {
    if (!course) {
      return null;
    }

    const classGroups = course.ClassGroups ?? [];
    const enrolledStudents = classGroups.reduce((total, group) => total + (group.ApprovedEnrollments ?? 0), 0);
    const capacity = classGroups.reduce((total, group) => total + (group.Capacity ?? 0), 0);

    return {
      id: course.Id,
      title: course.Title,
      mode: course.Mode,
      modeLabel: this.modeLabels[course.Mode] ?? 'Assincrono',
      category: course.CategoryName,
      instructor: course.InstructorName,
      published: course.IsPublished,
      publishedAt: course.PublishedAt,
      enrolledStudents,
      capacity,
      classGroups: classGroups.length
    };
  }

  private resolveStudentEnrollment(course: CourseDto, studentId: string) {
    const classGroups = course.ClassGroups ?? [];
    const enrollments = classGroups.flatMap(group => {
      const scopedEnrollments = group.Enrollments ?? [];
      return scopedEnrollments
        .filter(enrollment => enrollment.StudentId === studentId && [1, 2].includes(enrollment.Status))
        .map(enrollment => ({
          groupName: group.Name,
          status: enrollment.Status,
          date: enrollment.DecisionAt ?? enrollment.RequestedAt ?? new Date().toISOString()
        }));
    });

    if (!enrollments.length) {
      return null;
    }

    enrollments.sort((a, b) => {
      const priorityDiff = this.enrollmentPriority(b.status) - this.enrollmentPriority(a.status);
      if (priorityDiff !== 0) {
        return priorityDiff;
      }
      return new Date(b.date).getTime() - new Date(a.date).getTime();
    });

    return enrollments[0];
  }

  private enrollmentPriority(status: number): number {
    switch (status) {
      case 2:
        return 2;
      case 1:
        return 1;
      default:
        return 0;
    }
  }
}
