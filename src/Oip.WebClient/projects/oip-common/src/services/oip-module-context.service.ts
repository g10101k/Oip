import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { LayoutService } from './app.layout.service';
import { SecurityService } from './security.service';

export interface OipModuleContext {
  applicationCode: string;
  moduleCode: string;
  moduleInstanceId: number;
  userId: string;
  userName: string;
  roles: string[];
  permissions: string[];
  language: string;
  theme: string;
  apiBaseUrl: string;
}

@Injectable({ providedIn: 'root' })
export class OipModuleContextService {
  private readonly layoutService = inject(LayoutService);
  private readonly securityService = inject(SecurityService, { optional: true });
  private readonly context = new BehaviorSubject<OipModuleContext | null>(null);

  readonly context$: Observable<OipModuleContext | null> = this.context.asObservable();

  getContext(): OipModuleContext {
    const context = this.context.getValue();
    if (context) {
      return context;
    }

    const defaultContext = this.createDefaultContext();
    this.context.next(defaultContext);
    return defaultContext;
  }

  setContext(context: OipModuleContext): void {
    this.context.next(context);
  }

  patchContext(context: Partial<OipModuleContext>): void {
    this.context.next({ ...this.getContext(), ...context });
  }

  private createDefaultContext(): OipModuleContext {
    const user = this.securityService?.getCurrentUser?.();
    const roles = user?.roles ?? user?.realm_access?.roles ?? [];

    return {
      applicationCode: '',
      moduleCode: '',
      moduleInstanceId: 0,
      userId: user?.sub ?? user?.userName ?? user?.preferred_username ?? user?.email ?? '',
      userName: user?.displayName ?? user?.name ?? user?.userName ?? user?.preferred_username ?? user?.email ?? '',
      roles: Array.isArray(roles) ? roles : [],
      permissions: [],
      language: this.layoutService.language() ?? 'en',
      theme: this.layoutService.theme() ?? '',
      apiBaseUrl: window.location.origin
    };
  }
}
