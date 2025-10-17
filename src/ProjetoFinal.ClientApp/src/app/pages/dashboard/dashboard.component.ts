import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';

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

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent {
  private readonly router = inject(Router);

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

  goToCreateCourse(): void {
    this.router.navigate(['/courses/create']);
  }
}
