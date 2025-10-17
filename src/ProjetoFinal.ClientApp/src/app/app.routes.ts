import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'auth/login',
    loadComponent: () => import('./pages/auth/login/login.component').then(m => m.LoginComponent),
    title: 'Entrar - SES EAD'
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'dashboard'
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent),
    title: 'Painel - SES EAD',
    canActivate: [authGuard]
  },
  {
    path: 'courses/create',
    loadComponent: () => import('./pages/course-create/course-create.component').then(m => m.CourseCreateComponent),
    title: 'Criar Curso - SES EAD',
    canActivate: [authGuard]
  },
  {
    path: 'courses',
    loadComponent: () => import('./pages/courses/courses.component').then(m => m.CoursesComponent),
    title: 'Cursos - SES EAD',
    canActivate: [authGuard]
  },
  {
    path: 'class-groups',
    loadComponent: () => import('./pages/class-groups/class-groups.component').then(m => m.ClassGroupsComponent),
    title: 'Turmas - SES EAD',
    canActivate: [authGuard]
  },
  {
    path: 'activities',
    loadComponent: () => import('./pages/activities/activities.component').then(m => m.ActivitiesComponent),
    title: 'Atividades - SES EAD',
    canActivate: [authGuard]
  },
  {
    path: 'forum',
    loadComponent: () => import('./pages/forum/forum.component').then(m => m.ForumComponent),
    title: 'Forum - SES EAD',
    canActivate: [authGuard]
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];



