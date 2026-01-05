import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { instructorGuard } from './core/guards/instructor.guard';

export const routes: Routes = [
  {
    path: 'auth/login',
    loadComponent: () => import('./pages/auth/login/login.component').then(m => m.LoginComponent),
    title: 'Entrar - SES EAD'
  },
  {
    path: 'courses/:courseId/activities/:activityId',
    loadComponent: () =>
      import('./pages/course-activity-viewer/course-activity-viewer.component').then(m => m.CourseActivityViewerComponent),
    title: 'Atividade do Curso - SES EAD',
    canActivate: [authGuard]
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
    canActivate: [authGuard, instructorGuard]
  },
  {
    path: 'courses/:courseId/manage',
    loadComponent: () => import('./pages/course-manage/course-manage.component').then(m => m.CourseManageComponent),
    title: 'Gerenciar Curso - SES EAD',
    canActivate: [authGuard, instructorGuard]
  },
  {
    path: 'student/courses/:courseId',
    loadComponent: () =>
      import('./pages/student-course-view/student-course-view.component').then(m => m.StudentCourseViewComponent),
    title: 'Meu Curso - SES EAD',
    canActivate: [authGuard]
  },
  {
    path: 'courses/:courseId/contents/:contentId',
    loadComponent: () =>
      import('./pages/course-content-viewer/course-content-viewer.component').then(m => m.CourseContentViewerComponent),
    title: 'Conteudo do Curso - SES EAD',
    canActivate: [authGuard]
  },
  {
    path: 'courses',
    loadComponent: () => import('./pages/courses/courses.component').then(m => m.CoursesComponent),
    title: 'Cursos - SES EAD',
    canActivate: [authGuard]
  },
  {
    path: 'class-groups/:classGroupId/manage',
    loadComponent: () =>
      import('./pages/class-group-manage/class-group-manage.component').then(m => m.ClassGroupManageComponent),
    title: 'Gerenciar Turma - SES EAD',
    canActivate: [authGuard, instructorGuard]
  },
  {
    path: 'class-groups/create',
    loadComponent: () =>
      import('./pages/class-group-create/class-group-create.component').then(m => m.ClassGroupCreateComponent),
    title: 'Criar Turma - SES EAD',
    canActivate: [authGuard, instructorGuard]
  },
  {
    path: 'class-groups',
    loadComponent: () => import('./pages/class-groups/class-groups.component').then(m => m.ClassGroupsComponent),
    title: 'Turmas - SES EAD',
    canActivate: [authGuard, instructorGuard]
  },
  {
    path: 'activities',
    loadComponent: () => import('./pages/activities/activities.component').then(m => m.ActivitiesComponent),
    title: 'Atividades - SES EAD',
    canActivate: [authGuard]
  },
  {
    path: 'forum/threads/:threadId',
    loadComponent: () => import('./pages/forum-thread/forum-thread.component').then(m => m.ForumThreadComponent),
    title: 'Discussao do Forum - SES EAD',
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



