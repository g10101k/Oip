import { Component } from '@angular/core';
import { LogoComponent } from './logo.component';

@Component({
  selector: 'app-footer',
  template: `
    <div class="layout-footer">
      <logo class="mr-2" height="18" width="18"></logo>
      <span class="font-medium">OIP</span>
    </div>
  `,
  standalone: true,
  imports: [LogoComponent]
})
export class FooterComponent {
  constructor() {}
}
