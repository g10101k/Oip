import { EventEmitter, Injectable, Output } from '@angular/core';
import { OidcSecurityService } from "angular-auth-oidc-client";

@Injectable({
  providedIn: 'root'
})
export class OipSecurityService extends OidcSecurityService {

}

