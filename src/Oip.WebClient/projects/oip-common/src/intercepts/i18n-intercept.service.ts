import { inject } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';
import { switchMap, take } from 'rxjs/operators';
import { LayoutService } from '../services/app.layout.service';
import { SecurityService } from '../services/security.service';

export const langIntercept: HttpInterceptorFn = (req, next) => {
  const layoutService = inject(LayoutService);
  const securityService = inject(SecurityService);
  const lang = layoutService.language() ? layoutService.language() : 'en';
  const headers = req.headers
    .set('Accept-language', lang)
    .set('X-Timezone', layoutService.timeZone());
  const reqWithCredentials = req.clone({
    headers,
    withCredentials: true
  });

  if (!['POST', 'PUT', 'PATCH', 'DELETE'].includes(req.method.toUpperCase())) {
    return next(reqWithCredentials);
  }

  if (req.url.includes('/api/security/create-auth-session') || req.url.includes('/api/security/get-auth-csrf-token')) {
    return next(reqWithCredentials);
  }

  return securityService.getCsrfToken().pipe(
    take(1),
    switchMap((csrfToken) => next(csrfToken?.token
      ? reqWithCredentials.clone({
        headers: reqWithCredentials.headers.set(csrfToken.headerName, csrfToken.token)
      })
      : reqWithCredentials))
  );
};
