import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent),
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent),
  },
  {
    path: 'posts/new',
    loadComponent: () => import('./features/post-form/post-form.component').then(m => m.PostFormComponent),
    canActivate: [authGuard],
  },
  {
    path: 'posts/:id/edit',
    loadComponent: () => import('./features/post-form/post-form.component').then(m => m.PostFormComponent),
    canActivate: [authGuard],
  },
  {
    path: 'posts/:id',
    loadComponent: () => import('./features/post-detail/post-detail.component').then(m => m.PostDetailComponent),
  },
  {
    path: 'users/:username',
    loadComponent: () => import('./features/user-profile/user-profile.component').then(m => m.UserProfileComponent),
  },
  { path: '**', redirectTo: '' },
];
