import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

interface CourseCard {
  title: string;
  category: string;
  instructor: string;
  progress: number;
  students: number;
  mode: 'Assincrono' | 'Interactivo';
  status: 'Rascunho' | 'Publicado';
}

@Component({
  selector: 'app-courses',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './courses.component.html',
  styleUrl: './courses.component.css'
})
export class CoursesComponent {
  readonly courses: CourseCard[] = [
    {
      title: 'Transformacao Digital no Setor Publico',
      category: 'Inovacao',
      instructor: 'Ana Bezerra',
      progress: 64,
      students: 182,
      mode: 'Interactivo',
      status: 'Publicado'
    },
    {
      title: 'Metodologias Ativas de Aprendizagem',
      category: 'Pedagogia',
      instructor: 'Joao Rattes',
      progress: 92,
      students: 248,
      mode: 'Assincrono',
      status: 'Publicado'
    },
    {
      title: 'Gestao de Projetos Educacionais',
      category: 'Gestao',
      instructor: 'Mariana Prado',
      progress: 35,
      students: 86,
      mode: 'Interactivo',
      status: 'Rascunho'
    },
    {
      title: 'Comunicacao Inclusiva para Instrutores',
      category: 'Diversidade',
      instructor: 'Wallace Vidal',
      progress: 78,
      students: 96,
      mode: 'Assincrono',
      status: 'Publicado'
    }
  ];
}
