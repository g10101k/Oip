import { inject, Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { BaseDataService } from './base-data.service';
import { MenuChangeEvent } from '../events/menu-change.event';
import { MenuApi } from '../api/menu.api';
import { AppTitleService } from './app-title.service';
import {
  AddModuleInstanceDto,
  DeleteModuleInstanceParams,
  EditModuleInstanceDto,
  ModuleInstanceDto
} from '../api/data-contracts';

@Injectable()
export class MenuService extends BaseDataService {
  private readonly menuSource = new Subject<MenuChangeEvent>();
  private readonly resetSource = new Subject();
  private readonly titleService = inject(AppTitleService);
  private readonly menuDataService = inject(MenuApi);
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
    return this.menuDataService.get();
  }

  getAdminMenu() {
    return this.menuDataService.getAdminMenu();
  }

  getModules() {
    return this.menuDataService.getModules();
  }

  addModuleInstance(addModuleInstance: AddModuleInstanceDto) {
    return this.menuDataService.addModuleInstance(addModuleInstance as AddModuleInstanceDto);
  }

  deleteItem(moduleInstanceId: number) {
    return this.menuDataService.deleteModuleInstance({ id: moduleInstanceId } as DeleteModuleInstanceParams);
  }

  editModuleInstance(item: EditModuleInstanceDto) {
    return this.menuDataService.editModuleInstance(item);
  }
}
