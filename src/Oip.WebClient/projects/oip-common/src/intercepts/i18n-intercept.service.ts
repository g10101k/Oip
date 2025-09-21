import { inject } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';
import { LayoutService } from '../services/app.layout.service';

export const langIntercept: HttpInterceptorFn = (req, next) => {
  const layoutService = inject(LayoutService);
  const lang = layoutService.language() ? layoutService.language() : 'en';
  const httpHeaders = req.headers.set('Accept-language', lang);
  const authReq = req.clone({
    headers: httpHeaders
  });
  return next(authReq);
};
