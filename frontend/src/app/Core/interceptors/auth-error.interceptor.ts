import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((err) => {
      const isLoginCall = req.url.includes('/auth/login');
      if (!isLoginCall) {
        if (err.status === 401) {
          router.navigate(['/login']);
        } else if (err.status === 403) {
          router.navigate(['/forbidden']);
        }
      }
      return throwError(() => err);
    })
  );
};
