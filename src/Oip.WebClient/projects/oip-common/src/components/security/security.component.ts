import { Component, effect, inject, input } from '@angular/core';
import { HttpClient } from '../../api/http-client';
import { AccessRightDto } from '../../dtos/object-acl.dto';
import { SecurityDto } from '../../dtos/security.dto';
import { MsgService } from '../../services/msg.service';
import { ObjectAclEditorComponent } from '../object-acl-editor/object-acl-editor.component';

/** Object type of a module instance, see ModuleAccessControl on the server. */
export const MODULE_INSTANCE_OBJECT_TYPE = 'oip.module-instance';

/** Route of the access control controller of the module instances. */
export const MODULE_ACCESS_CONTROL_API = 'module-access-control';

/**
 * Security tab of a module instance: the access control list of the instance, with the rights the module declares.
 * Folder rights are inherited unless the inheritance is switched off.
 */
@Component({
  selector: 'security',
  standalone: true,
  imports: [ObjectAclEditorComponent],
  template: `
    @if (id() != null) {
      <div class="card">
        <oip-object-acl-editor
          [apiBase]="apiBase"
          [objectId]="id()!"
          [objectType]="objectType"
          [rights]="rights" />
      </div>
    }
  `
})
export class SecurityComponent {
  private readonly httpClient = inject(HttpClient);
  private readonly msgService = inject(MsgService);
  private loadToken = 0;

  protected readonly apiBase = MODULE_ACCESS_CONTROL_API;
  protected readonly objectType = MODULE_INSTANCE_OBJECT_TYPE;
  protected rights: AccessRightDto[] | null = null;

  /** Module instance. */
  readonly id = input<number | undefined>(undefined);
  /** Route of the module controller, used to read the rights the module declares. */
  readonly controller = input<string | undefined>(undefined);

  constructor() {
    effect(() => {
      void this.loadRights(this.controller());
    });
  }

  private async loadRights(controller: string | undefined): Promise<void> {
    const token = ++this.loadToken;
    this.rights = null;
    if (!controller) return;

    try {
      const declared = await this.httpClient.request<SecurityDto[]>({
        path: `/api/${controller}/get-module-rights`,
        method: 'GET',
        secure: true,
        format: 'json'
      });
      if (token !== this.loadToken) return;
      this.rights = declared.map((right) => ({
        code: right.code,
        name: right.name,
        description: right.description
      }));
    } catch (error) {
      if (token === this.loadToken) this.msgService.errorFromException(error, 'Load module rights error');
    }
  }
}
