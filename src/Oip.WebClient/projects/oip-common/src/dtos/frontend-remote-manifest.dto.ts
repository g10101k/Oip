export enum FrontendIntegrationType {
  InternalRoute = 'InternalRoute',
  Iframe = 'Iframe',
  FederatedRemote = 'FederatedRemote',
  WebComponent = 'WebComponent'
}

export enum FrontendRemoteEntryKind {
  Routes = 'Routes',
  Component = 'Component'
}

export enum FrontendModuleErrorState {
  InvalidManifest = 'InvalidManifest',
  RemoteUnavailable = 'RemoteUnavailable',
  IncompatibleVersion = 'IncompatibleVersion',
  InvalidRemoteExport = 'InvalidRemoteExport',
  PermissionDenied = 'PermissionDenied',
  RuntimeError = 'RuntimeError'
}
