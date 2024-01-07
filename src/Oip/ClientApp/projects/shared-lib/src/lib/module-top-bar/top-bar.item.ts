// TopBarItem interface
export interface TopBarItem {
  // Identifier of item
  id: string;
  // Icon from prime icon
  icon: string;
  // Caption
  caption: string;
  // Claims from SSO
  claims?: string[];

  // Method from module call when index toolbar change
  click?(): void;
}
