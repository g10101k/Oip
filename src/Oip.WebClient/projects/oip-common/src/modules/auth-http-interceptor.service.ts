import {inject, Injectable} from "@angular/core";
import {HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from "@angular/common/http";
import {SecurityService} from "../services/security.service";
import {Observable} from "rxjs";

@Injectable()
export class AuthHttpInterceptor implements HttpInterceptor {
  private securityService = inject(SecurityService);

  intercept(req: HttpRequest<any>, handler: HttpHandler): Observable<HttpEvent<any>> {
    let response = this.securityService.loginResponse.getValue();
    if (response) {
      const authReq = req.clone({
        headers: req.headers.set('Authorization', 'Bearer ' + response.accessToken)
      });
      return handler.handle(authReq);
    }
    return handler.handle(req);
  }
}
