import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

interface ActivityItem {
  title: string;
  dueDate: string;
  course: string;
  classGroup: string;
  submissions: { received: number; total: number };
  allowLate: boolean;
}

@Component({
  selector: 'app-activities',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './activities.component.html',
  styleUrl: './activities.component.css'
})
export class ActivitiesComponent {
  readonly activities: ActivityItem[] = [
    {
      title: 'Projeto de diagnostico digital',
      dueDate: 'Entrega: 04/out · 18h',
      course: 'Transformacao Digital',
      classGroup: 'Turma A',
      submissions: { received: 18, total: 36 },
      allowLate: true
    },
    {
      title: 'Plano de aula com metodologias ativas',
      dueDate: 'Entrega: 06/out · 23h59',
      course: 'Metodologias Ativas',
      classGroup: 'Turma B',
      submissions: { received: 12, total: 28 },
      allowLate: false
    },
    {
      title: 'Feedback em video',
      dueDate: 'Entrega: 09/out · 20h',
      course: 'Comunicacao Inclusiva',
      classGroup: 'Turma piloto',
      submissions: { received: 4, total: 25 },
      allowLate: true
    }
  ];
}
