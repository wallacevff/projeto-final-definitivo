import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { CoursesService } from '../../core/services/courses.service';
import { ClassGroupListItem } from '../../core/api/courses.api';

@Component({
  selector: 'app-class-groups',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './class-groups.component.html',
  styleUrl: './class-groups.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClassGroupsComponent {
  private readonly service = inject(CoursesService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly classGroups = signal<ClassGroupListItem[]>([]);

  constructor() {
    this.service
      .getClassGroupRows()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (groups: ClassGroupListItem[]) => {
          this.classGroups.set(groups);
          this.error.set(null);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Nao foi possivel carregar as turmas.');
          this.loading.set(false);
        }
      });
  }

  trackByGroupId(_: number, item: ClassGroupListItem): string {
    return item.id;
  }
}

