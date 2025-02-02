import { Component } from '@angular/core';
import { MenuComponent } from './menu.component';

@Component({
  selector: 'app-sidebar',
  template: `
    <app-menu></app-menu>`,
  standalone: true,
  imports: [
    MenuComponent
  ]
})
export class SidebarComponent {
    constructor() { }
}
