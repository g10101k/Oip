import { inject, Injectable } from '@angular/core';
import { ContentType, HttpClient } from '../api/http-client';
import { ObjectAclDto, SubjectDto, UpdateObjectAclRequest } from '../dtos/object-acl.dto';

/**
 * Calls the access control API of a service. `apiBase` is the route of the service controller derived from
 * BaseAccessControlController, for example `customer-access-control`.
 */
@Injectable({ providedIn: 'root' })
export class ObjectRightsService {
  private readonly httpClient = inject(HttpClient);
  private readonly rightsCache = new Map<string, Promise<string[]>>();

  /** Rights of the current user on an object, cached until {@link invalidate} is called. */
  getRights(apiBase: string, objectType: string, objectId: number): Promise<string[]> {
    const key = `${apiBase}|${objectType}|${objectId}`;
    let rights = this.rightsCache.get(key);
    if (!rights) {
      rights = this.httpClient
        .request<string[]>({
          path: `/api/${apiBase}/get-effective-rights-by-object`,
          method: 'GET',
          query: { objectType, objectId },
          secure: true,
          format: 'json'
        })
        .catch((error) => {
          this.rightsCache.delete(key);
          throw error;
        });
      this.rightsCache.set(key, rights);
    }
    return rights;
  }

  /** Forgets the cached rights, for example after the access control list was changed. */
  invalidate(): void {
    this.rightsCache.clear();
  }

  getAcl(apiBase: string, objectType: string, objectId: number): Promise<ObjectAclDto> {
    return this.httpClient.request<ObjectAclDto>({
      path: `/api/${apiBase}/get-object-acl-by-object`,
      method: 'GET',
      query: { objectType, objectId },
      secure: true,
      format: 'json'
    });
  }

  async updateAcl(apiBase: string, request: UpdateObjectAclRequest): Promise<void> {
    await this.httpClient.request<unknown>({
      path: `/api/${apiBase}/update-object-acl`,
      method: 'PUT',
      body: request,
      secure: true,
      type: ContentType.Json
    });
    this.invalidate();
  }

  searchSubjects(apiBase: string, objectType: string, objectId: number, filter: string): Promise<SubjectDto[]> {
    return this.httpClient.request<SubjectDto[]>({
      path: `/api/${apiBase}/get-subjects-by-filter`,
      method: 'GET',
      query: { objectType, objectId, filter },
      secure: true,
      format: 'json'
    });
  }
}
