import { IsActiveMatchOptions, Params, QueryParamsHandling } from "@angular/router";

export interface ContextMenuItemDto {
  class: string | string[] | Set<string> | { [p: string]: any } | null | undefined;
  command: any;
  disabled: boolean;
  fragment: string | undefined;
  icon: string | string[] | Set<string> | { [p: string]: any } | null | undefined;
  items: ContextMenuItemDto[];
  label: string;
  preserveFragment: unknown;
  queryParamsHandling: QueryParamsHandling | null | undefined;
  routerLink: string;
  routerLinkActiveOptions: { exact: boolean } | IsActiveMatchOptions;
  visible: boolean;
  skipLocationChange: unknown;
  queryParams: Params | null | undefined;
  target: string;
  state: {[p: string]: any} | undefined;
  replaceUrl: unknown;
  badgeClass: string;
}
