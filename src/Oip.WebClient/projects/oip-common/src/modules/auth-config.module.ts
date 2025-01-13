import {inject, Injectable, NgModule} from '@angular/core';
import {
  AbstractSecurityStorage, AuthInterceptor,
  AuthModule,
  StsConfigHttpLoader,
  StsConfigLoader
} from 'angular-auth-oidc-client';
import {SecurityStorageService} from "../services/security-storage.service";
import {
  HTTP_INTERCEPTORS,
  HttpClient,
  HttpEvent,
  HttpHandler, HttpHeaders,
  HttpInterceptor,
  HttpRequest
} from "@angular/common/http";
import {map} from "rxjs/operators";
import {Observable} from 'rxjs';
import {SecurityService} from "../services/security.service";

@Injectable()
export class LoggingInterceptor implements HttpInterceptor {
  private q = inject(SecurityService);

  intercept(req: HttpRequest<any>, handler: HttpHandler): Observable<HttpEvent<any>> {
    let response = this.q.loginResponse.getValue();
    req.headers.set('Authorization', 'Bearer ' + response.accessToken);

    console.log('Request URL: ' + req.url);
    return handler.handle(req);
  }
}


/**
 * Load keycloak settings from backend and save to sessionStorage
 * @param httpClient
 * @returns StsConfigHttpLoader
 */
export const httpLoaderFactory = (httpClient: HttpClient) => {
  const KEYCLOAK_SETTINGS_KEY = 'keycloak-client-settings';
  let settingsSting = sessionStorage.getItem(KEYCLOAK_SETTINGS_KEY);
  if (settingsSting) {
    let config$ = new Observable<any>((sub) => {
      sub.next(JSON.parse(settingsSting));
    });
    return new StsConfigHttpLoader(config$);
  } else {
    const config$ = httpClient.get<any>(`api/security/get-keycloak-client-settings`).pipe(
      map((config: any) => {
        let authConfig = {
          authority: config.authority,
          redirectUrl: window.location.origin,
          postLogoutRedirectUri: window.location.origin,
          clientId: config.clientId,
          scope: config.scope,
          responseType: config.responseType,
          useRefreshToken: config.useRefreshToken,
          silentRenew: config.silentRenew,
          logLevel: config.logLevel,
        };
        sessionStorage.setItem(KEYCLOAK_SETTINGS_KEY, JSON.stringify(authConfig));
        return authConfig;
      })
    );
    return new StsConfigHttpLoader(config$);
  }
}

@NgModule({
  imports: [
    AuthModule.forRoot({
      loader: {
        provide: StsConfigLoader,
        useFactory: httpLoaderFactory,
        deps: [HttpClient],
      },

    })
  ],
  providers: [
    {provide: AbstractSecurityStorage, useClass: SecurityStorageService},
    {provide: HTTP_INTERCEPTORS, useClass: LoggingInterceptor, multi: true},
  ],

  exports: [AuthModule]
})
export class AuthConfigModule {
}


