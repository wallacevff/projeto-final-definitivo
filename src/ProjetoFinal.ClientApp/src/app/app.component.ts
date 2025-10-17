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
    { label: 'Painel', description: 'Resumo geral da plataforma', icon: '📊', route: '/dashboard' },
    { label: 'Cursos', description: 'Catálogo e gestão de cursos', icon: '📚', route: '/courses' },
    { label: 'Turmas', description: 'Acompanhe suas turmas ativas', icon: '👥', route: '/class-👥' },
    { label: 'Atividades', description: 'Programe e avalie atividades', icon: '📝', route: '/activities' },
    { label: 'Fórum', description: 'Discussões entre alunos e instrutores', icon: '💬', route: '/💬' },
  ];

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }
}
