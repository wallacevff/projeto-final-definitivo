import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'dashboard'
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent),
    title: 'Painel · SES EAD'
  },
  {
    path: 'courses',
    loadComponent: () => import('./pages/coming-soon/coming-soon.component').then(m => m.ComingSoonComponent),
    data: {
      comingSoon: {
        title: 'Gestão de cursos',
        description: 'Cadastre cursos, organize trilhas e acompanhe métricas de engajamento do catálogo.',
        meta: 'Planejado para Módulo 1'
      }
    },
    title: 'Cursos · SES EAD'
  },
  {
    path: 'class-groups',
    loadComponent: () => import('./pages/coming-soon/coming-soon.component').then(m => m.ComingSoonComponent),
    data: {
      comingSoon: {
        title: 'Turmas em desenvolvimento',
        description: 'Gerencie inscrições, vagas e comunicação com os participantes de cada turma.',
        meta: 'Design de interface em andamento'
      }
    },
    title: 'Turmas · SES EAD'
  },
  {
    path: 'activities',
    loadComponent: () => import('./pages/coming-soon/coming-soon.component').then(m => m.ComingSoonComponent),
    data: {
      comingSoon: {
        title: 'Atividades avaliativas',
        description: 'Crie e acompanhe atividades síncronas e assíncronas, inclusive com vídeo e fóruns.',
        meta: 'Integração com backend pendente'
      }
    },
    title: 'Atividades · SES EAD'
  },
  {
    path: 'forum',
    loadComponent: () => import('./pages/coming-soon/coming-soon.component').then(m => m.ComingSoonComponent),
    data: {
      comingSoon: {
        title: 'Fórum colaborativo',
        description: 'Promova discussões entre alunos e instrutores, com moderação e anexos multimídia.',
        meta: 'Entrega prevista no próximo ciclo'
      }
    },
    title: 'Fórum · SES EAD'
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
