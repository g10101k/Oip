import {inject, Injectable} from '@angular/core';
import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import {lastValueFrom, map, Observable} from 'rxjs';
import {OpenIdConfiguration, StsConfigHttpLoader} from "angular-auth-oidc-client";


/**
 * BaseDataService provides a unified interface for sending HTTP requests
 * using Angular's HttpClient. It supports standard HTTP methods and automatic
 * credential handling.
 */
@Injectable({  providedIn: 'root'})
export class ConfigService {
  public readonly config: OpenIdConfiguration;

  constructor() {
    const KEYCLOAK_SETTINGS_KEY = 'keycloak-client-settings';
    let settingsStings = localStorage.getItem(KEYCLOAK_SETTINGS_KEY);
    if (settingsStings) {
      this.config = JSON.parse(settingsStings);
    } else {
      fetch(`api/security/get-keycloak-client-settings`)
        .then(res => res.json())
        .then((config: any) => {
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
          localStorage.setItem(KEYCLOAK_SETTINGS_KEY, JSON.stringify(authConfig));
        });
    }
  }

  getConfig(): OpenIdConfiguration {
    return this.config;
  }
}
