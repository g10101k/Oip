import { Injectable, InjectionToken, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ToastMessageOptions } from 'primeng/api';
import { AppTitleService } from './app-title.service';
import { MsgService } from './msg.service';
import { OipModuleContext, OipModuleContextService } from './oip-module-context.service';

export type OipToast = ToastMessageOptions;

export interface OipShellApi {
  getContext(): OipModuleContext;
  hasPermission(permission: string): boolean;
  showToast(message: OipToast): void;
  setPageTitle(title: string): void;
  navigate(url: string): void;
  reportError(error: unknown): void;
}

export const OIP_SHELL_API = new InjectionToken<OipShellApi>('OIP_SHELL_API');

@Injectable({ providedIn: 'root' })
export class OipShellApiService implements OipShellApi {
  private readonly contextService = inject(OipModuleContextService);
  private readonly msgService = inject(MsgService);
  private readonly titleService = inject(AppTitleService);
  private readonly router = inject(Router);

  getContext(): OipModuleContext {
    return this.contextService.getContext();
  }

  hasPermission(permission: string): boolean {
    const context = this.getContext();
    return context.permissions.includes(permission);
  }

  showToast(message: OipToast): void {
    this.msgService.add(message);
  }

  setPageTitle(title: string): void {
    this.titleService.setTitle(title);
  }

  navigate(url: string): void {
    if (url.startsWith('/')) {
      this.router.navigateByUrl(url);
      return;
    }

    this.router.navigate([url]);
  }

  reportError(error: unknown): void {
    console.error(error);
    this.msgService.errorFromException(error, 'Module error');
  }
}
