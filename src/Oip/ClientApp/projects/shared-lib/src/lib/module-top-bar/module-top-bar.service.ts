import { EventEmitter, Injectable, Output } from '@angular/core';
import { TopBarItem } from "./top-bar.item";

@Injectable({
  providedIn: 'root'
})
export class ModuleTopBarService {

  public topBarItems: TopBarItem[] = [];

  private _activeIndex: number;

  public get availableTopBarItems():  TopBarItem[]
  {
    // Todo: https://github.com/g10101k/Oip/issues/43
    return this.topBarItems;
  }

  get activeTopBarItem(): TopBarItem | undefined {
    return this.topBarItems[this._activeIndex];
  }

  get activeIndex(): number {
    return this._activeIndex;
  }

  set activeIndex(value: number) {
    this._activeIndex = value;
    if (this.activeTopBarItem.click)
      this.activeTopBarItem.click();
  }

  constructor() {
  }

  // Set tob bar items
  setTopBarItems(items: TopBarItem[]) {
    this.topBarItems = items;
  }

  checkId(id: string): boolean {
    if (this.activeTopBarItem === undefined && id === 'content')
      return true;
    if (this.activeTopBarItem === undefined)
      return false;
    return this.activeTopBarItem.id == id;
  }
}

