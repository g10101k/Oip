import { inject } from "@angular/core";
import { HttpInterceptorFn } from "@angular/common/http";
import { LayoutService } from "../services/app.layout.service";

export const langIntercept: HttpInterceptorFn = (req, next) => {
  let layoutService = inject(LayoutService);
  let lang = layoutService.language() ? layoutService.language() : 'en';
  let httpHeaders = req.headers.set('Accept-language', lang);
  const authReq = req.clone({
    headers: httpHeaders
  });
  return next(authReq);
};

