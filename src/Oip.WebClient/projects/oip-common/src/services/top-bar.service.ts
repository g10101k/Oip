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
    console.debug('[OIP topbar] activeId setter', JSON.stringify({
      previous: this._activeId,
      next: value,
      items: this.topBarItems.map((item) => item.id)
    }));
    this._activeId = value;
    const activeTopBarItem = this.activeTopBarItem;
    console.debug('[OIP topbar] active item resolved', JSON.stringify({
      activeId: this._activeId,
      item: activeTopBarItem?.id,
      hasClick: typeof activeTopBarItem?.click === 'function'
    }));
    if (activeTopBarItem?.click) activeTopBarItem.click();
    this.activeIdSubject.next(value);
    console.debug('[OIP topbar] activeId emitted', JSON.stringify({value}));
  }

  constructor() {}

  // Set tob bar items
  setTopBarItems(items: TopBarDto[]) {
    console.debug('[OIP topbar] setTopBarItems', JSON.stringify({
      previous: this.topBarItems.map((item) => item.id),
      next: items.map((item) => item.id)
    }));
    this.topBarItems = items;
  }

  checkId(id: string): boolean {
    const activeTopBarItem = this.activeTopBarItem;
    const result = activeTopBarItem === undefined
      ? id === 'content'
      : activeTopBarItem.id == id;
    console.debug('[OIP topbar] checkId', JSON.stringify({
      requested: id,
      activeId: this._activeId,
      activeItem: activeTopBarItem?.id,
      result
    }));
    return result;
  }
}
