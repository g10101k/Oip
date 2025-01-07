import { Component } from '@angular/core';
import { LayoutService } from "oip/common";

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  standalone: true
})
export class FooterComponent {
  constructor(public layoutService: LayoutService) {
  }
}
