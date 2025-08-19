import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection,
} from '@angular/core';

import { routes } from './app.routes';
import Aura from '@primeuix/themes/aura';

import { providePrimeNG } from 'primeng/config';
import { provideRouter } from '@angular/router';
import { tokenInterceptor } from './Core/interceptors/token.interceptor';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { authErrorInterceptor } from './Core/interceptors/auth-error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: Aura,
        options: {
          cssLayer: {
            name: 'primeng',
            order: 'tailwind, base, primeng ',
          },
        },
      },
    }),
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([tokenInterceptor, authErrorInterceptor])
    ),
    provideBrowserGlobalErrorListeners(),
  ],
};
