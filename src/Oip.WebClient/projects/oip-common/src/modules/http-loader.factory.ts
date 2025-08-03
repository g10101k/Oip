import {HttpClient} from "@angular/common/http";
import {map, Observable, lastValueFrom, firstValueFrom, of} from "rxjs";
import {OpenIdConfiguration, StsConfigHttpLoader} from "angular-auth-oidc-client";
import {ConfigService} from "../services/config.service";

/**
 * Load keycloak settings from backend and save to sessionStorage
 * @param config
 * @returns StsConfigHttpLoader
 */
export const httpLoaderAuthFactory = (config: ConfigService) => {
  let q = config.getConfig();
  return new StsConfigHttpLoader(of(q));
}
