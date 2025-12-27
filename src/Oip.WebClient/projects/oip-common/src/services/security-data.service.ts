import { Injectable } from '@angular/core';
import { BaseDataService } from './base-data.service';
import { SecurityDto } from '../dtos/security.dto';
import { PutSecurityDto } from '../dtos/put-security.dto';

@Injectable()
export class SecurityDataService extends BaseDataService {
  getSecurity(controller: string, id: number) {
    return this.sendRequest<SecurityDto[]>(this.baseUrl + `api/${controller}/get-security?id=${id}`);
  }

  saveSecurity(controller: string, request: PutSecurityDto) {
    return this.sendRequest<any>(this.baseUrl + `api/${controller}/put-security`, 'PUT', request);
  }

  getRealmRoles() {
    return this.sendRequest<string[]>(this.baseUrl + `api/security/get-realm-roles`);
  }
}
