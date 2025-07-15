import { inject, Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Title } from "@angular/platform-browser";

/**
 * A route guard that ensures the user is authenticated and has a valid access token.
 * If the access token is expired, it attempts to refresh the session.
 * If authentication fails or refresh is unsuccessful, redirects to the unauthorized page.
 */
@Injectable({ providedIn: 'root' })
export class AppTitleService {
  private readonly title = inject(Title);
  private titleSubject = new BehaviorSubject<string>('');
  public title$ = this.titleSubject.asObservable();

  /**
   * Get the title of the current HTML document.
   */
  getTitle(): string {
    return this.titleSubject.value;
  }

  /**
   * Set the title of the current HTML document.
   * @param newTitle
   */
  setTitle(newTitle: string): void {
    this.title.setTitle(newTitle);
    this.titleSubject.next(newTitle);
  }
}
