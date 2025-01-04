import { Injectable } from '@angular/core';
import { BaseDataService, PutSecurityDto, SecurityDto } from "common";

@Injectable()
export class SecurityDataService extends BaseDataService {
  getSecurity(id: number) {
    return this.sendRequest<SecurityDto[]>(this.baseUrl + `api/weatherforecast/get-security?id=${id}`);
  }

  saveSecurity(request: PutSecurityDto) {
    return this.sendRequest<any>(this.baseUrl + `api/weatherforecast/put-security`, 'PUT', request);
  }

  getRealmRoles() {
    return this.sendRequest<string[]>(`api/security/get-realm-roles`);
  }
}
