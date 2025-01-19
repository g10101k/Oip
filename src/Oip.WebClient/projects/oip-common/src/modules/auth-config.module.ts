import { NgModule } from '@angular/core';
import { AbstractSecurityStorage, AuthModule, StsConfigLoader } from 'angular-auth-oidc-client';
import { SecurityStorageService } from "../services/security-storage.service";
import { HTTP_INTERCEPTORS, HttpClient } from "@angular/common/http";
import { AuthHttpInterceptor } from "./auth-http-interceptor.service";
import { httpLoaderFactory } from "./http-loader.factory";

@NgModule({
  imports: [
    AuthModule.forRoot({
      loader: { provide: StsConfigLoader, useFactory: httpLoaderFactory, deps: [HttpClient], },
    })
  ],
  providers: [
    { provide: AbstractSecurityStorage, useClass: SecurityStorageService },
    { provide: HTTP_INTERCEPTORS, useClass: AuthHttpInterceptor, multi: true },
  ],

  exports: [AuthModule]
})
export class AuthConfigModule {
}


