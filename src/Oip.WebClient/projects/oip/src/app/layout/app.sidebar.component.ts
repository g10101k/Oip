import { Component, ElementRef } from '@angular/core';
import { AppMenuComponent } from './app.menu.component';
import { AppConfigService } from "./service/appconfigservice";

@Component({
    selector: 'app-sidebar',
    templateUrl: './app.sidebar.component.html',
    imports: [AppMenuComponent]
})
export class AppSidebarComponent {
  constructor(public layoutService: AppConfigService, public el: ElementRef) {
  }
}

