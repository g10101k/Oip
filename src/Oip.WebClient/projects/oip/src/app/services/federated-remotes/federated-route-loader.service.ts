import { Injectable, Type } from '@angular/core';
import { Route, Routes } from '@angular/router';
import { FrontendRemoteManifestDto } from 'oip-common';
import { RemoteLoadErrorComponent } from '../../components/remote-load-error/remote-load-error.component';
import { RemoteCompatibilityService } from './remote-compatibility.service';

declare const __webpack_init_sharing__: ((scope: string) => Promise<void>) | undefined;
declare const __webpack_share_scopes__: Record<string, unknown> | undefined;

type WebpackContainer = {
  init: (shareScope: unknown) => Promise<void> | void;
  get: (module: string) => Promise<() => unknown>;
};

@Injectable({ providedIn: 'root' })
export class FederatedRouteLoaderService {
  private readonly loadedRemoteEntries = new Map<string, Promise<void>>();

  constructor(private readonly compatibilityService: RemoteCompatibilityService) {}

  async loadRoutes(manifest: FrontendRemoteManifestDto): Promise<Routes> {
    try {
      this.assertFederatedManifest(manifest);

      const compatibility = this.compatibilityService.validate(manifest);
      if (!compatibility.compatible) {
        throw new Error(compatibility.reason ?? 'Remote module version is not compatible with current shell.');
      }

      await this.loadRemoteEntry(manifest.remoteEntryUrl!);
      const exposed = await this.loadExposedModule(manifest.remoteName!, manifest.exposedModule!);

      if (this.isRoutesEntry(manifest.entryKind)) {
        const routes = this.resolveRoutes(exposed);
        return routes;
      }

      const component = this.resolveComponent(exposed);
      return [{ path: '', component }];
    } catch (error) {
      console.error('Remote module load error', manifest, error);
      return this.createErrorRoutes(manifest, error);
    }
  }

  private assertFederatedManifest(manifest: FrontendRemoteManifestDto): void {
    const missingFields = [
      ['remoteEntryUrl', manifest.remoteEntryUrl],
      ['remoteName', manifest.remoteName],
      ['exposedModule', manifest.exposedModule]
    ]
      .filter(([, value]) => !value)
      .map(([field]) => field);

    if (missingFields.length > 0) {
      throw new Error(`Remote manifest is missing required fields: ${missingFields.join(', ')}.`);
    }
  }

  private async loadRemoteEntry(remoteEntryUrl: string): Promise<void> {
    const existing = this.loadedRemoteEntries.get(remoteEntryUrl);
    if (existing) {
      return existing;
    }

    const loadPromise = new Promise<void>((resolve, reject) => {
      const script = document.createElement('script');
      script.src = remoteEntryUrl;
      script.type = 'text/javascript';
      script.async = true;
      script.onload = () => resolve();
      script.onerror = () => reject(new Error(`Remote entry is not available: ${remoteEntryUrl}.`));
      document.head.appendChild(script);
    });

    this.loadedRemoteEntries.set(remoteEntryUrl, loadPromise);
    return loadPromise;
  }

  private async loadExposedModule(remoteName: string, exposedModule: string): Promise<unknown> {
    if (typeof __webpack_init_sharing__ === 'function') {
      await __webpack_init_sharing__('default');
    }

    const container = this.getContainer(remoteName);
    const shareScope = typeof __webpack_share_scopes__ === 'object' ? __webpack_share_scopes__['default'] : {};
    await container.init(shareScope);

    const factory = await container.get(exposedModule);
    return factory();
  }

  private getContainer(remoteName: string): WebpackContainer {
    const container = (window as unknown as Record<string, WebpackContainer | undefined>)[remoteName];
    if (!container) {
      throw new Error(`Remote container '${remoteName}' was not found.`);
    }

    return container;
  }

  private resolveRoutes(exposed: unknown): Routes {
    const module = exposed as Record<string, unknown>;
    const routes = module['remoteRoutes'] ?? module['routes'] ?? module['default'];

    if (!Array.isArray(routes)) {
      throw new Error('Remote module does not export Routes.');
    }

    return routes as Routes;
  }

  private resolveComponent(exposed: unknown): Type<unknown> {
    const module = exposed as Record<string, unknown>;
    const component = module['RemoteComponent'] ?? module['Component'] ?? module['default'];

    if (!component) {
      throw new Error('Remote module does not export a component.');
    }

    return component as Type<unknown>;
  }

  private isRoutesEntry(entryKind: unknown): boolean {
    return `${entryKind ?? 'Routes'}`.toLowerCase() === 'routes';
  }

  private createErrorRoutes(manifest: FrontendRemoteManifestDto, error: unknown): Routes {
    const route: Route = {
      path: '',
      component: RemoteLoadErrorComponent,
      data: {
        remoteManifest: manifest,
        remoteLoadError: error instanceof Error ? error.message : 'Remote module load error.'
      }
    };

    return [route];
  }
}
