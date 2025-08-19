import { inject } from '@angular/core';
import { CanMatchFn, Router, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const landingGuard: CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) {
    return router.createUrlTree(['/login']);
  }

  const isAdmin = auth.hasPermission('admin:access') || auth.hasRole('Admin');
  return isAdmin
    ? router.createUrlTree(['/admin'])
    : router.createUrlTree(['/dashboard']);
};
