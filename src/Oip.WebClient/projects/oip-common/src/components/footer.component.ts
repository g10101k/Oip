import { Component } from '@angular/core';
import { LogoComponent } from "./logo.component";

@Component({
  selector: 'app-footer',
  template: `
    <div class="layout-footer">
      <logo width="18" height="18" class="mr-2"></logo>
      <span class="font-medium">OIP</span>
    </div>
  `,
  standalone: true,
  imports: [
    LogoComponent
  ]
})
export class FooterComponent {
  constructor() {
  }
}
