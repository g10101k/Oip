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

export const httpLoaderFactory = (httpClient: HttpClient) => {
  const config$ = httpClient.get<any>(`api/security/get-keycloak-client-settings`).pipe(
    map((config: any) => {
      console.log(config);
      return {
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
    })
  );
  return new StsConfigHttpLoader(config$);
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
