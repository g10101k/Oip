export interface OipExtensionManifest {
  key: string;
  name: string;
  version: string;
  routePath: string;
  loadType?: OipExtensionLoadType;
  elementName?: string | null;
  scriptUrl?: string | null;
  remoteEntryUrl?: string | null;
  exposedModule?: string | null;
  componentName?: string | null;
  apiBaseUrl: string;
  icon?: string | null;
  description?: string | null;
  permissions?: unknown;
  settingsSchema?: unknown;
}

export type OipExtensionLoadType = 'moduleFederation' | 'customElement';

export interface OipExtensionModuleMetadata {
  moduleId: number;
  name: string;
  settings?: string | null;
  kind: number;
  manifestUrl?: string | null;
  extensionKey?: string | null;
  loadType?: OipExtensionLoadType | null;
  elementName?: string | null;
  scriptUrl?: string | null;
  remoteEntryUrl?: string | null;
  exposedModule?: string | null;
  componentName?: string | null;
  apiBaseUrl?: string | null;
  version?: string | null;
}

export interface OipExtensionHostContext {
  moduleInstanceId: number | undefined;
  extensionKey: string;
  apiBasePath: string;
  settings: unknown;
  locale?: string;
  theme?: unknown;
  user?: unknown;
  permissions: {
    canRead: boolean;
    canEdit: boolean;
    canDelete: boolean;
  };
}

export interface OipExtensionNotifyEvent {
  severity?: 'success' | 'info' | 'warn' | 'error';
  summary?: string;
  detail?: string;
}

export interface OipExtensionNavigateEvent {
  commands?: unknown[];
  url?: string;
}
