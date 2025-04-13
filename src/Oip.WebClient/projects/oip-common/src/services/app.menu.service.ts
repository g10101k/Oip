import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { BaseDataService } from "./base-data.service";
import { MenuChangeEvent } from "../events/menu-change.event";
import { AddModuleInstanceDto } from "./../dtos/add-module-instance.dto";
import { EditModuleInstanceDto } from "../dtos/edit-module-instance.dto";
import { ContextMenuItemDto } from "../dtos/context-menu-item.dto";

@Injectable({
  providedIn: 'root'
})
export class MenuService extends BaseDataService {

  private readonly menuSource = new Subject<MenuChangeEvent>();
  private readonly resetSource = new Subject();

  menuSource$ = this.menuSource.asObservable();
  resetSource$ = this.resetSource.asObservable();
  contextMenuItem: any;

  onMenuStateChange(event: MenuChangeEvent) {
    this.menuSource.next(event);
  }

  reset() {
    this.resetSource.next(true);
  }

  getMenu() {
    return this.sendRequest<ContextMenuItemDto[]>(this.baseUrl + 'api/menu/get');
  }

  getAdminMenu() {
    return this.sendRequest<ContextMenuItemDto[]>(this.baseUrl + 'api/menu/get-admin-menu');
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


