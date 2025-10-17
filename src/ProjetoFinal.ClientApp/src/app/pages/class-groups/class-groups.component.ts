import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

interface ClassGroupRow {
  name: string;
  course: string;
  seats: { total: number; occupied: number };
  requiresApproval: boolean;
  status: 'Em andamento' | 'Inscricoes abertas' | 'Concluida';
  nextEvent: string;
}

@Component({
  selector: 'app-class-groups',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './class-groups.component.html',
  styleUrl: './class-groups.component.css'
})
export class ClassGroupsComponent {
  readonly classGroups: ClassGroupRow[] = [
    {
      name: 'Turma A · Transformacao Digital',
      course: 'Transformacao Digital no Setor Publico',
      seats: { total: 40, occupied: 36 },
      requiresApproval: true,
      status: 'Em andamento',
      nextEvent: 'Mentoria · 02/out · 19h'
    },
    {
      name: 'Turma B · Metodologias Ativas',
      course: 'Metodologias Ativas de Aprendizagem',
      seats: { total: 50, occupied: 28 },
      requiresApproval: false,
      status: 'Inscricoes abertas',
      nextEvent: 'Live de abertura · 05/out · 10h'
    },
    {
      name: 'Turma piloto · Comunicacao Inclusiva',
      course: 'Comunicacao Inclusiva para Instrutores',
      seats: { total: 25, occupied: 25 },
      requiresApproval: true,
      status: 'Concluida',
      nextEvent: 'Entrega de certificados'
    }
  ];
}
