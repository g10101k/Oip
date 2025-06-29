import { inject, Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { BaseDataService } from "./base-data.service";
import { MenuChangeEvent } from "../events/menu-change.event";
import { AddModuleInstanceDto } from "../dtos/add-module-instance.dto";
import { EditModuleInstanceDto } from "../dtos/edit-module-instance.dto";
import { ContextMenuItemDto } from "../dtos/context-menu-item.dto";
import { Title } from "@angular/platform-browser";
import { Menu } from "../api/Menu";

@Injectable({ providedIn: 'root' })
export class MenuService extends BaseDataService {
  private readonly menuSource = new Subject<MenuChangeEvent>();
  private readonly resetSource = new Subject();
  private readonly titleService = inject(Title);
  private readonly menuDataService = inject(Menu);
  menuSource$ = this.menuSource.asObservable();
  resetSource$ = this.resetSource.asObservable();
  contextMenuItem: any;

  public menu: any[] = [];
  public adminMode: boolean = false;

  async loadMenu() {
    this.menu = (this.adminMode) ? await this.getAdminMenu() : await this.getMenu();
  }

  /**
   * Handles changes in the menu state.
   * @param {MenuChangeEvent} event - The event containing information about the menu state change.
   * @return {void}
   */
  onMenuStateChange(event: MenuChangeEvent): void {
    this.menuSource.next(event);
    this.titleService.setTitle(event.item.label);
  }

  reset() {
    this.resetSource.next(true);
  }

  getMenu() {
    return this.menuDataService.menuGet();
  }

  getAdminMenu() {
    return this.menuDataService.menuGetAdminMenu();
  }

  getModules() {
    return this.sendRequest<any>(this.baseUrl + 'api/menu/get-modules');
  }

  addModuleInstance(addModuleInstance: AddModuleInstanceDto) {
    return this.sendRequest(this.baseUrl + 'api/menu/add-module-instance', "POST", addModuleInstance);
  }

  deleteItem(moduleInstanceId: number) {
    return this.sendRequest(this.baseUrl + 'api/menu/delete-module-instance?id=' + moduleInstanceId, "DELETE");
  }

  editModuleInstance(item: EditModuleInstanceDto) {
    return this.sendRequest(this.baseUrl + 'api/menu/edit-module-instance', "POST", item);
  }
}


