import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin, of } from 'rxjs';
import { distinctUntilChanged, filter, map, switchMap, tap } from 'rxjs/operators';

import { CoursesService } from '../../core/services/courses.service';
import { ActivitiesService } from '../../core/services/activities.service';
import { CourseContentsService } from '../../core/services/course-contents.service';
import { ForumService } from '../../core/services/forum.service';
import { CourseDto } from '../../core/api/courses.api';
import { ActivityListItem } from '../../core/api/activities.api';
import { CourseContentListItem } from '../../core/api/contents.api';
import { ForumThreadListItem } from '../../core/api/forum.api';

type CourseSection = 'activities' | 'contents' | 'forum';

@Component({
  selector: 'app-student-course-view',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './student-course-view.component.html',
  styleUrl: './student-course-view.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StudentCourseViewComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly coursesService = inject(CoursesService);
  private readonly activitiesService = inject(ActivitiesService);
  private readonly contentsService = inject(CourseContentsService);
  private readonly forumService = inject(ForumService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly course = signal<CourseDto | null>(null);
  readonly activities = signal<ActivityListItem[]>([]);
  readonly contents = signal<CourseContentListItem[]>([]);
  readonly forumThreads = signal<ForumThreadListItem[]>([]);
  readonly sections: { key: CourseSection; label: string; description: string }[] = [
    {
      key: 'activities',
      label: 'Atividades',
      description: 'Prazos, entregas e avaliacoes publicadas para este curso.'
    },
    {
      key: 'contents',
      label: 'Conteudos',
      description: 'Materiais publicados pelo instrutor, em video, texto ou anexos.'
    },
    {
      key: 'forum',
      label: 'Forum',
      description: 'Discussoes e anuncios compartilhados com a turma.'
    }
  ];
  readonly activeSection = signal<CourseSection>('activities');

  constructor() {
    this.route.paramMap
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        map(params => params.get('courseId')),
        filter((courseId): courseId is string => Boolean(courseId)),
        distinctUntilChanged(),
        switchMap(courseId => {
          this.loading.set(true);
          this.error.set(null);
          this.activeSection.set('activities');
          this.activities.set([]);
          this.contents.set([]);
          this.forumThreads.set([]);

          return this.coursesService.getCourseById(courseId).pipe(
            tap(course => this.course.set(course)),
            switchMap(course => {
              const courseLookup = new Map<string, string>([[course.Id, course.Title]]);

              const forum$ = course.EnableForum
                ? this.forumService.getThreads(courseLookup, { CourseId: course.Id })
                : of<ForumThreadListItem[]>([]);

              return forkJoin({
                activities: this.activitiesService.getActivities({ CourseId: course.Id, VisibleToStudents: true }),
                contents: this.contentsService.getContents({ CourseId: course.Id }),
                forumThreads: forum$
              });
            })
          );
        })
      )
      .subscribe({
        next: ({ activities, contents, forumThreads }) => {
          this.activities.set(activities);
          this.contents.set(contents);
          this.forumThreads.set(forumThreads);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Nao foi possivel carregar os dados do curso.');
          this.loading.set(false);
        }
      });
  }

  setSection(section: CourseSection): void {
    this.activeSection.set(section);
  }

  backToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }

  openForum(): void {
    this.router.navigate(['/forum']);
  }
}
