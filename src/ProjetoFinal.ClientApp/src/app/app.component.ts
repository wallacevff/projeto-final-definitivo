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
    { label: 'Painel', description: 'Resumo geral da plataforma', icon: 'bi bi-speedometer2', route: '/dashboard' },
    { label: 'Cursos', description: 'Catalogo e gestao de cursos', icon: 'bi bi-mortarboard', route: '/courses' },
    { label: 'Turmas', description: 'Acompanhe suas turmas ativas', icon: 'bi bi-people', route: '/class-groups' },
    { label: 'Atividades', description: 'Programe e avalie atividades', icon: 'bi bi-clipboard-check', route: '/activities' },
    { label: 'Forum', description: 'Discussoes entre alunos e instrutores', icon: 'bi bi-chat-dots', route: '/forum' },
  ];

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }
}

