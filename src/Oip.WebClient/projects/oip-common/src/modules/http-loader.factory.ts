import { of } from "rxjs";
import { StsConfigHttpLoader } from "../oids"
import { ConfigService } from "../services/config.service";

/**
 * Load keycloak settings from backend and save to sessionStorage
 * @param config
 * @returns StsConfigHttpLoader
 */
export const httpLoaderAuthFactory = (config: ConfigService) => {
  let q = config.getConfig();
  return new StsConfigHttpLoader(q);
}
