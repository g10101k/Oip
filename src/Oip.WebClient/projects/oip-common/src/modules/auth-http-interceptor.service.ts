import { inject, Injectable } from "@angular/core";
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { SecurityService } from "../services/security.service";
import { Observable } from "rxjs";
import { LayoutService } from "../services/app.layout.service";

@Injectable()
export class AuthHttpInterceptor implements HttpInterceptor {
  private readonly securityService = inject(SecurityService);
  private readonly layoutService = inject(LayoutService);

  intercept(req: HttpRequest<any>, handler: HttpHandler): Observable<HttpEvent<any>> {
    let response = this.securityService.loginResponse.getValue();
    let lang = this.layoutService.language() ? this.layoutService.language() : 'en';
    if (response) {
      let headers = req.headers
        .set('Authorization', 'Bearer ' + response.accessToken)
        .set('Accept-language', lang);
      const authReq = req.clone({
        headers: headers
      });

      return handler.handle(authReq);
    }
    return handler.handle(req);
  }
}
