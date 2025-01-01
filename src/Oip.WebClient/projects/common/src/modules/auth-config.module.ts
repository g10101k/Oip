import { NgModule } from '@angular/core';
import { AuthModule, LogLevel } from 'angular-auth-oidc-client';

@NgModule({
  imports: [
      AuthModule.forRoot({
      config: {
        authority: 'https://s-gbt-wsn-00010:8443/realms/oip',
        redirectUrl: window.location.origin,
        postLogoutRedirectUri: window.location.origin,
        clientId: 'oip-client',
        scope: 'openid profile email offline_access roles',
        responseType: 'code',
        silentRenew: true,
        useRefreshToken: true,
        logLevel: LogLevel.Debug,
      },
    })
  ],
  exports: [AuthModule],
})
export class AuthConfigModule {
}
