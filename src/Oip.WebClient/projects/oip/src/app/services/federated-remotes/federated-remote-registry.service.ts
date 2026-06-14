import { Injectable, inject } from '@angular/core';
import { Route, Router, Routes } from '@angular/router';
import { ApplicationsApi, AuthGuardService, FrontendRemoteManifestDto, OipModuleContextService } from 'oip-common';
import { FederatedRouteLoaderService } from './federated-route-loader.service';

@Injectable({ providedIn: 'root' })
export class FederatedRemoteRegistryService {
  private readonly applicationsApi = inject(ApplicationsApi);
  private readonly authGuard = inject(AuthGuardService);
  private readonly contextService = inject(OipModuleContextService);
  private readonly routeLoader = inject(FederatedRouteLoaderService);
  private readonly router = inject(Router);

  async registerRoutes(): Promise<void> {
    let manifests: FrontendRemoteManifestDto[] = [];

    try {
      manifests = await this.applicationsApi.getFrontendModuleManifests();
    } catch (error) {
      console.error('Frontend module manifests were not loaded.', error);
      return;
    }

    const remoteRoutes = manifests
      .filter((manifest) => manifest.enabled !== false)
      .map((manifest) => this.createRoute(manifest))
      .filter((route): route is Route => route !== null);

    if (remoteRoutes.length === 0) {
      return;
    }

    this.router.resetConfig(this.mergeRemoteRoutes(this.router.config, remoteRoutes));
  }

  private createRoute(manifest: FrontendRemoteManifestDto): Route | null {
    const path = this.normalizeRoutePath(manifest.routePath);
    if (!path) {
      console.error('Frontend remote manifest does not define routePath.', manifest);
      return null;
    }

    return {
      path,
      data: {
        frontendRemoteManifestCode: manifest.code
      },
      canActivate: [(_, state) => this.authGuard.canActivate(state.url)],
      loadChildren: async () => {
        this.contextService.patchContext({
          moduleCode: manifest.code ?? '',
          permissions: manifest.permissions ?? []
        });

        return this.routeLoader.loadRoutes(manifest);
      }
    };
  }

  private mergeRemoteRoutes(routes: Routes, remoteRoutes: Routes): Routes {
    return routes.map((route) => {
      if (route.path === '' && Array.isArray(route.children)) {
        return {
          ...route,
          children: this.upsertRoutes(route.children, remoteRoutes)
        };
      }

      return route;
    });
  }

  private upsertRoutes(existingRoutes: Routes, remoteRoutes: Routes): Routes {
    const staticRoutes = existingRoutes.filter((route) => !route.data?.['frontendRemoteManifestCode']);
    const staticPaths = new Set(staticRoutes.map((route) => route.path));
    const routesToAdd = remoteRoutes.filter((route) => {
      if (!staticPaths.has(route.path)) {
        return true;
      }

      console.error(`Frontend remote route '${route.path}' conflicts with a shell route and was skipped.`);
      return false;
    });

    return [...staticRoutes, ...routesToAdd];
  }

  private normalizeRoutePath(routePath: string | null | undefined): string {
    return routePath?.trim().replace(/^\/+|\/+$/g, '') ?? '';
  }
}
