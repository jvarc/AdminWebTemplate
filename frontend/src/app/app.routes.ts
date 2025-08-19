import { Routes } from '@angular/router';
import { authGuard } from './Core/guards/auth.guard';
import { permissionGuard } from './Core/guards/permission.guard';
import { guestOnlyGuard } from './Core/guards/guest.guard';
import { landingGuard } from './Core/guards/landing.guard';

export const routes: Routes = [
  {
    path: 'login',
    canMatch: [guestOnlyGuard],
    loadComponent: () =>
      import('./features/auth/login/login.component').then(
        (m) => m.LoginComponent
      ),
  },
  {
    path: 'admin',
    canMatch: [authGuard, permissionGuard(['admin:access'])],
    data: {
      menu: {
        label: 'Administración',
        icon: 'pi pi-shield',
        showInUserMenu: true,
        requiredPerms: ['admin:access'],
      },
    },
    loadComponent: () =>
      import('./features/admin/admin.component').then((m) => m.AdminComponent),
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'users',
      },
      {
        path: 'users',
        // lectura
        canMatch: [permissionGuard(['users:read'])],
        loadComponent: () =>
          import('./features/admin/users/users.component').then(
            (m) => m.UsersComponent
          ),
      },
      {
        path: 'roles',
        // libre o añade un permiso si lo expones (p. ej. roles:write)
        loadComponent: () =>
          import('./features/admin/roles/roles.component').then(
            (m) => m.RolesComponent
          ),
      },
    ],
  },
  {
    path: 'dashboard',
    canMatch: [authGuard],
    data: {
      menu: {
        label: 'Dashboard',
        icon: 'pi pi-home',
        showInUserMenu: true,
        requiredPerms: [],
      },
    },
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then(
        (m) => m.DashboardComponent
      ),
  },
  {
    path: 'forbidden',
    loadComponent: () =>
      import('./features/forbidden/forbidden.component').then(
        (m) => m.ForbiddenComponent
      ),
  },
  { path: '', canMatch: [landingGuard], children: [] },
  { path: '**', canMatch: [landingGuard], children: [] },
];
