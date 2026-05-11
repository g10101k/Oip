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
    if (this.activeTopBarItem?.click) this.activeTopBarItem.click();
    this.activeIdSubject.next(value);
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
