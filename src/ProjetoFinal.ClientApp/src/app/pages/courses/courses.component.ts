import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { CoursesService } from '../../core/services/courses.service';
import { CourseListItem } from '../../core/api/courses.api';

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
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly courses = signal<CourseListItem[]>([]);

  constructor() {
    this.service
      .getCourseCards()
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

  trackByCourseId(_: number, item: CourseListItem): string {
    return item.id;
  }

  percentOccupied(course: CourseListItem): number {
    if (!course.capacity) {
      return 0;
    }

    return Math.min(100, Math.round((course.enrolledStudents / course.capacity) * 100));
  }
}
