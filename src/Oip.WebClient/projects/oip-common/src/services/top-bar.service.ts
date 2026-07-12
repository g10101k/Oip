import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { TopBarDto } from '../dtos/top-bar.dto';

@Injectable({
  providedIn: 'root'
})
export class TopBarService {
  topBarItems: TopBarDto[] = [];
  private readonly activeIdSubject = new Subject<string | undefined>();

  readonly activeId$ = this.activeIdSubject.asObservable();

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
    const activeTopBarItem = this.activeTopBarItem;
    if (activeTopBarItem?.click) activeTopBarItem.click();
    this.activeIdSubject.next(value);
  }

  constructor() {}

  // Set tob bar items
  setTopBarItems(items: TopBarDto[]) {
    this.topBarItems = items;
  }

  checkId(id: string): boolean {
    const activeTopBarItem = this.activeTopBarItem;
    const result = activeTopBarItem === undefined
      ? id === 'content'
      : activeTopBarItem.id == id;
    return result;
  }
}
