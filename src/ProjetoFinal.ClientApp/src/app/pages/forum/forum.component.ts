import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { ForumService } from '../../core/services/forum.service';
import { CoursesService } from '../../core/services/courses.service';
import { ForumThreadListItem } from '../../core/api/forum.api';
import { CourseDto } from '../../core/api/courses.api';

@Component({
  selector: 'app-forum',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './forum.component.html',
  styleUrl: './forum.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ForumComponent {
  private readonly forumService = inject(ForumService);
  private readonly coursesService = inject(CoursesService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly threads = signal<ForumThreadListItem[]>([]);

  constructor() {
    this.coursesService
      .getCoursesDto()
      .pipe(
        switchMap(courses => {
          const mapEntries = courses.map((course: CourseDto) => [course.Id, course.Title] as const);
          return this.forumService.getThreads(new Map(mapEntries));
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (threads: ForumThreadListItem[]) => {
          this.threads.set(threads);
          this.error.set(null);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Nao foi possivel carregar os topicos do forum.');
          this.loading.set(false);
        }
      });
  }

  trackByThread(_: number, item: ForumThreadListItem): string {
    return item.id;
  }
}
