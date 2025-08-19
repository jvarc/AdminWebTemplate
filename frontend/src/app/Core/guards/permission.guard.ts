// permission.guard.ts
import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export function permissionGuard(required: string[] | string): CanMatchFn {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (!auth.isLoggedIn()) {
      return router.createUrlTree(['/login']);
    }

    return auth.hasAllPermissions(required)
      ? true
      : router.createUrlTree(['/forbidden']);
  };
}
