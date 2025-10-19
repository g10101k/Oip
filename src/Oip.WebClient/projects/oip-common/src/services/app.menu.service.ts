import { inject, Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { BaseDataService } from './base-data.service';
import { MenuChangeEvent } from '../events/menu-change.event';
import { EditModuleInstanceDto } from '../dtos/edit-module-instance.dto';
import { Menu } from '../api/Menu';
import { AppTitleService } from './app-title.service';
import { AddModuleInstanceDto, ModuleInstanceDto } from '../api/data-contracts';

@Injectable()
export class MenuService extends BaseDataService {
  private readonly menuSource = new Subject<MenuChangeEvent>();
  private readonly resetSource = new Subject();
  private readonly titleService = inject(AppTitleService);
  private readonly menuDataService = inject(Menu);
  menuSource$ = this.menuSource.asObservable();
  resetSource$ = this.resetSource.asObservable();
  contextMenuItem: any;

  public menu: ModuleInstanceDto[] = [];
  public adminMode: boolean = false;

  async loadMenu() {
    this.menu = this.adminMode ? await this.getAdminMenu() : await this.getMenu();
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
    return this.menuDataService.menuGetModules();
  }

  addModuleInstance(addModuleInstance: AddModuleInstanceDto) {
    return this.menuDataService.menuAddModuleInstance(addModuleInstance as AddModuleInstanceDto);
  }

  deleteItem(moduleInstanceId: number) {
    return this.sendRequest(this.baseUrl + 'api/menu/delete-module-instance?id=' + moduleInstanceId, 'DELETE');
  }

  editModuleInstance(item: EditModuleInstanceDto) {
    return this.sendRequest(this.baseUrl + 'api/menu/edit-module-instance', 'POST', item);
  }
}
