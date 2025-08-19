import { inject } from '@angular/core';
import { CanMatchFn, Router, Route, UrlSegment } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanMatchFn = (
  _route: Route,
  _segments: UrlSegment[]
) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isLoggedIn()) return true;
  router.navigate(['/login']);
  return false;
};
