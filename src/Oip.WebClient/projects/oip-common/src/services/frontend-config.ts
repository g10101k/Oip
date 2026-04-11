import { InjectionToken } from '@angular/core';

export type OipFrontendAppMode = 'standalone' | 'distributed';

export interface OipFrontendConfig {
  appMode: OipFrontendAppMode;
  apiBaseUrl?: string;
  notificationHubUrl?: string;
}

export const DEFAULT_OIP_FRONTEND_CONFIG: OipFrontendConfig = {
  appMode: 'standalone',
  apiBaseUrl: '',
  notificationHubUrl: ''
};

export const OIP_FRONTEND_CONFIG = new InjectionToken<OipFrontendConfig>(
  'OIP_FRONTEND_CONFIG',
  {
    factory: () => DEFAULT_OIP_FRONTEND_CONFIG
  }
);
