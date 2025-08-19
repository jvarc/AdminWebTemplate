import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const guestOnlyGuard: CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) return true;

  const isAdmin = auth.hasPermission('admin:access') || auth.hasRole('Admin');
  return router.createUrlTree([isAdmin ? '/admin' : '/dashboard']);
};
