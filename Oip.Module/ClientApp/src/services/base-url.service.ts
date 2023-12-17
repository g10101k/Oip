import {Injectable} from '@angular/core';

@Injectable({
  // declares that this service should be created
  // by the root application injector.
  providedIn: 'root',
})
export class BaseUrlService {
  public getUrl() {
    return document.getElementsByTagName('base')[0].href;
  }
}
