import { Component } from '@angular/core';
import { LayoutService } from "oip/common";

@Component({
  selector: 'app-footer',
  templateUrl: './app.footer.component.html',
  standalone: true
})
export class AppFooterComponent {
  constructor(public layoutService: LayoutService) {
  }
}
