import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { ActivitiesService } from '../../core/services/activities.service';
import { ActivityListItem } from '../../core/api/activities.api';

@Component({
  selector: 'app-activities',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './activities.component.html',
  styleUrl: './activities.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ActivitiesComponent {
  private readonly service = inject(ActivitiesService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly activities = signal<ActivityListItem[]>([]);

  constructor() {
    this.service
      .getActivities()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (activities: ActivityListItem[]) => {
          this.activities.set(activities);
          this.error.set(null);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Nao foi possivel carregar as atividades.');
          this.loading.set(false);
        }
      });
  }

  trackByActivity(_: number, item: ActivityListItem): string {
    return item.id;
  }
}

