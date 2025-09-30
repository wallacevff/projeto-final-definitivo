import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

interface NavItem {
  label: string;
  description: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'SES EAD';
  isSidebarCollapsed = false;

  readonly navItems: NavItem[] = [
    { label: 'Painel', description: 'Resumo geral da plataforma', icon: 'DB', route: '/dashboard' },
    { label: 'Cursos', description: 'Catalogo e gestao de cursos', icon: 'CR', route: '/courses' },
    { label: 'Turmas', description: 'Acompanhe suas turmas ativas', icon: 'TG', route: '/class-groups' },
    { label: 'Atividades', description: 'Programe e avalie atividades', icon: 'AT', route: '/activities' },
    { label: 'Forum', description: 'Discussoes entre alunos e instrutores', icon: 'FR', route: '/forum' },
  ];

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }
}
