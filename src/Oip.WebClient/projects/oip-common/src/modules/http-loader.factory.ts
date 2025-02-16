import {HttpClient} from "@angular/common/http";
import {map, Observable} from "rxjs";
import { OpenIdConfiguration, StsConfigHttpLoader } from "angular-auth-oidc-client";

/**
 * Load keycloak settings from backend and save to sessionStorage
 * @param httpClient
 * @returns StsConfigHttpLoader
 */
export const httpLoaderAuthFactory = (httpClient: HttpClient) => {
  const KEYCLOAK_SETTINGS_KEY = 'keycloak-client-settings';
  let settingsStings = sessionStorage.getItem(KEYCLOAK_SETTINGS_KEY);
  if (settingsStings) {
    let config$ = new Observable<any>((subscribe) => {
      subscribe.next(JSON.parse(settingsStings));
    });
    return new StsConfigHttpLoader(config$);
  } else {
    const config$ = httpClient.get<any>(`api/security/get-keycloak-client-settings`).pipe(
      map((config: any) => {
        let authConfig: OpenIdConfiguration = {
          authority: config.authority,
          redirectUrl: window.location.origin,
          postLogoutRedirectUri: window.location.origin,
          clientId: config.clientId,
          scope: config.scope,
          responseType: config.responseType,
          useRefreshToken: config.useRefreshToken,
          silentRenew: config.silentRenew,
          logLevel: config.logLevel,
          secureRoutes: config.secureRoutes,
        };
        sessionStorage.setItem(KEYCLOAK_SETTINGS_KEY, JSON.stringify(authConfig));
        return authConfig;
      })
    );
    return new StsConfigHttpLoader(config$);
  }
}
