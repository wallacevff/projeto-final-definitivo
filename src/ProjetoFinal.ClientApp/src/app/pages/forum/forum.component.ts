import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

interface ForumThreadCard {
  title: string;
  course: string;
  replies: number;
  lastMessage: string;
  author: string;
  isPinned: boolean;
  isLocked: boolean;
}

@Component({
  selector: 'app-forum',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './forum.component.html',
  styleUrl: './forum.component.css'
})
export class ForumComponent {
  readonly threads: ForumThreadCard[] = [
    {
      title: 'Ideias para acelerar a transformacao digital nas escolas',
      course: 'Transformacao Digital',
      replies: 42,
      lastMessage: 'ha 2 horas · Camila Santos',
      author: 'Ana Bezerra',
      isPinned: true,
      isLocked: false
    },
    {
      title: 'Compartilhe exemplos de metodologias ativas bem-sucedidas',
      course: 'Metodologias Ativas',
      replies: 18,
      lastMessage: 'ha 35 minutos · Joao Lima',
      author: 'Joao Rattes',
      isPinned: false,
      isLocked: false
    },
    {
      title: 'Checklist de acessibilidade para materiais didaticos',
      course: 'Comunicacao Inclusiva',
      replies: 12,
      lastMessage: 'ha 1 hora · Wallace Vidal',
      author: 'Wallace Vidal',
      isPinned: false,
      isLocked: true
    }
  ];
}
