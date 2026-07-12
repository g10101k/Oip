import { OipExtensionHostContext, OipExtensionNavigateEvent, OipExtensionNotifyEvent } from './extension-host.types';

export const OIP_EXTENSION_EVENTS = {
  contextChange: 'oip:context-change',
  titleChange: 'oip:title-change',
  settingsChange: 'oip:settings-change',
  notify: 'oip:notify',
  navigate: 'oip:navigate',
  error: 'oip:error'
} as const;

export function emitOipTitleChange(element: HTMLElement, title: string): void {
  element.dispatchEvent(new CustomEvent(OIP_EXTENSION_EVENTS.titleChange, { detail: title, bubbles: true }));
}

export function emitOipSettingsChange(element: HTMLElement, settings: unknown): void {
  element.dispatchEvent(new CustomEvent(OIP_EXTENSION_EVENTS.settingsChange, { detail: settings, bubbles: true }));
}

export function emitOipNotify(element: HTMLElement, notification: OipExtensionNotifyEvent): void {
  element.dispatchEvent(new CustomEvent(OIP_EXTENSION_EVENTS.notify, { detail: notification, bubbles: true }));
}

export function emitOipNavigate(element: HTMLElement, navigation: OipExtensionNavigateEvent): void {
  element.dispatchEvent(new CustomEvent(OIP_EXTENSION_EVENTS.navigate, { detail: navigation, bubbles: true }));
}

export function emitOipError(element: HTMLElement, error: unknown): void {
  element.dispatchEvent(new CustomEvent(OIP_EXTENSION_EVENTS.error, { detail: error, bubbles: true }));
}

export function emitOipContextChange(element: HTMLElement, context: OipExtensionHostContext): void {
  element.dispatchEvent(new CustomEvent(OIP_EXTENSION_EVENTS.contextChange, { detail: context }));
}
