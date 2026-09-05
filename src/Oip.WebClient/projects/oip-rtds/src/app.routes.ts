import { oipAuthGuard, provideOipRoutes } from 'oip-common';

export const appRoutes = provideOipRoutes({
  children: [
    {
      path: 'tag-management/:id',
      loadComponent: () =>
        import('./app/components/tag-management/tag-management.component').then((m) => m.TagManagement),
      canActivate: [oipAuthGuard]
    }
  ],
  features: { dbMigration: 'rtds-meta-data-context-migration-module/:id' }
});
