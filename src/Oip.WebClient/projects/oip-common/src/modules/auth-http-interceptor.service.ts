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
    let token: string = null;
    this.securityService.getAccessToken().subscribe(x => {
        token = x;
      }
    );
    let lang = this.layoutService.language() ? this.layoutService.language() : 'en';
    let httpHeaders = req.headers.set('Accept-language', lang);
    if (token) {
       httpHeaders = httpHeaders
        .set('Authorization', 'Bearer ' + token);
    }
    const authReq = req.clone({
      headers: httpHeaders
    });
    return handler.handle(authReq);  }
}
