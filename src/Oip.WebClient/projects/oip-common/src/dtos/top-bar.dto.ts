// TopBarDto interface
export interface TopBarDto {
  // Identifier of item
  id: 'content' | 'settings' | 'security';
  // Icon from prime icon
  icon: string;
  // Caption
  caption: string;
  // Claims from SSO
  claims?: string[];
  // Method from module call when index toolbar change
  click?(): void;
}
