import { NgModule } from '@angular/core';
import {
  AbstractSecurityStorage,
  AuthModule,
  StsConfigHttpLoader,
  StsConfigLoader
} from 'angular-auth-oidc-client';
import { SecurityStorageService } from "../services/security-storage.service";
import { HttpClient } from "@angular/common/http";
import { map } from "rxjs/operators";
import { Observable } from 'rxjs';


export const httpLoaderFactory = (httpClient: HttpClient) => {
  const KEYCLOAK_SETTINGS_KEY = 'keycloak-client-settings';

  let settingsSting = sessionStorage.getItem(KEYCLOAK_SETTINGS_KEY);
  if (settingsSting) {
    let config$ = new Observable<any>((sub) => {
      sub.next(JSON.parse(settingsSting));
    });
    return new StsConfigHttpLoader(config$);
  }
  else {
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
  providers: [{ provide: AbstractSecurityStorage, useClass: SecurityStorageService }],
  exports: [AuthModule]
})
export class AuthConfigModule {
}
