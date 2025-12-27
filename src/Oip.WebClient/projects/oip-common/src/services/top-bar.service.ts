import { Injectable } from '@angular/core';
import { TopBarDto } from '../dtos/top-bar.dto';

@Injectable({
  providedIn: 'root'
})
export class TopBarService {
  topBarItems: TopBarDto[] = [];

  private _activeId: string | undefined;

  get availableTopBarItems(): TopBarDto[] {
    return this.topBarItems;
  }

  get activeTopBarItem(): TopBarDto | undefined {
    if (this._activeId === undefined) {
      return undefined;
    }
    return this.topBarItems.find((topBarItem: TopBarDto) => topBarItem.id === this._activeId);
  }

  get activeId(): string | undefined {
    return this._activeId;
  }

  set activeId(value: string | undefined) {
    this._activeId = value;
    if (this.activeTopBarItem?.click) this.activeTopBarItem.click();
  }

  constructor() {}

  // Set tob bar items
  setTopBarItems(items: TopBarDto[]) {
    this.topBarItems = items;
  }

  checkId(id: string): boolean {
    if (this.activeTopBarItem === undefined && id === 'content') return true;
    if (this.activeTopBarItem === undefined) return false;
    return this.activeTopBarItem.id == id;
  }
}
