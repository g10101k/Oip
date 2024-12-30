import { Injectable } from '@angular/core';
import { TopBarDto } from "../dtos/top-bar.dto";

@Injectable({
  providedIn: 'root'
})
export class TopBarService {

  topBarItems: TopBarDto[] = [];

  private _activeIndex: number | undefined;

  get availableTopBarItems(): TopBarDto[] {
    return this.topBarItems;
  }

  get activeTopBarItem(): TopBarDto | undefined {
    if (this._activeIndex === undefined) {
      return undefined;
    }
    return this.topBarItems[this._activeIndex];
  }

  get activeIndex(): number | undefined {
    return this._activeIndex;
  }

  set activeIndex(value: number) {
    this._activeIndex = value;
    if (this.activeTopBarItem?.click)
      this.activeTopBarItem.click();
  }

  constructor() {
  }

  // Set tob bar items
  setTopBarItems(items: TopBarDto[]) {
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

