import { inject, Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Title } from "@angular/platform-browser";

/**
 * Service to manage the application title.
 */
@Injectable({ providedIn: 'root' })
export class AppTitleService {
  private readonly title = inject(Title);
  private titleSubject = new BehaviorSubject<string>('');
  public title$ = this.titleSubject.asObservable();

  /**
   * Set the title of the current HTML document.
   * @param newTitle
   */
  setTitle(newTitle: string): void {
    this.title.setTitle(newTitle);
    this.titleSubject.next(newTitle);
  }
}
