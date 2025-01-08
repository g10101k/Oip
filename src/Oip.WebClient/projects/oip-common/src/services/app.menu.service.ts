import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { BaseDataService } from "./base-data.service";
import { MenuChangeEvent } from "../events/menu-change.event";

@Injectable({
  providedIn: 'root'
})
export class MenuService extends BaseDataService {

  private menuSource = new Subject<MenuChangeEvent>();
  private resetSource = new Subject();

  menuSource$ = this.menuSource.asObservable();
  resetSource$ = this.resetSource.asObservable();

  onMenuStateChange(event: MenuChangeEvent) {
    this.menuSource.next(event);
  }

  reset() {
    this.resetSource.next(true);
  }

  getMenu() {
    return this.sendRequest<any>(this.baseUrl + 'api/menu/get');
  }
}


