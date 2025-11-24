import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';

import { AuthService } from '../services/auth.service';

export const instructorGuard: CanActivateFn = (_route, state): boolean | UrlTree => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const user = authService.currentUser();
  if (user?.role === 2) {
    return true;
  }

  const fallbackUrl = user ? '/dashboard' : '/auth/login';
  if (!user) {
    authService.setRedirectUrl(state.url);
  }

  return router.parseUrl(fallbackUrl);
};
