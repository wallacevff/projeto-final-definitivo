import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthService } from './core/services/auth.service';

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
  private readonly authService = inject(AuthService);

  title = 'SES EAD';
  isSidebarCollapsed = false;

  readonly navItems: NavItem[] = [
    { label: 'Painel', description: 'Resumo geral da plataforma', icon: 'bi bi-speedometer2', route: '/dashboard' },
    { label: 'Cursos', description: 'Catalogo e gestao de cursos', icon: 'bi bi-mortarboard', route: '/courses' },
    { label: 'Turmas', description: 'Acompanhe suas turmas ativas', icon: 'bi bi-people', route: '/class-groups' },
    { label: 'Atividades', description: 'Programe e avalie atividades', icon: 'bi bi-clipboard-check', route: '/activities' },
    { label: 'Forum', description: 'Discussoes entre alunos e instrutores', icon: 'bi bi-chat-dots', route: '/forum' },
  ];

  readonly currentUser = this.authService.currentUser;
  readonly userInitials = computed(() => {
    const user = this.currentUser();
    if (!user) {
      return '--';
    }
    const source = user.fullName || user.username;
    return source
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map(part => part[0]?.toUpperCase() ?? '')
      .join('');
  });
  readonly roleLabel = computed(() => {
    const user = this.currentUser();
    return user ? this.authService.getRoleLabel(user.role) : '';
  });

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }

  isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  isInstructor(): boolean {
    return this.authService.isInstructorRole();
  }

  isStudent(): boolean {
    return this.authService.isStudentRole();
  }

  logout(): void {
    this.authService.logout();
  }
}

