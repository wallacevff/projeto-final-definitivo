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
    loadComponent: () => import('./pages/courses/courses.component').then(m => m.CoursesComponent),
    title: 'Cursos · SES EAD'
  },
  {
    path: 'class-groups',
    loadComponent: () => import('./pages/class-groups/class-groups.component').then(m => m.ClassGroupsComponent),
    title: 'Turmas · SES EAD'
  },
  {
    path: 'activities',
    loadComponent: () => import('./pages/activities/activities.component').then(m => m.ActivitiesComponent),
    title: 'Atividades · SES EAD'
  },
  {
    path: 'forum',
    loadComponent: () => import('./pages/forum/forum.component').then(m => m.ForumComponent),
    title: 'Fórum · SES EAD'
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
