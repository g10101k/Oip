import { Component } from '@angular/core';
import { LayoutService } from "../../services/app.layout.service";
import { LogoComponent } from '../logo/logo.component';

@Component({
    selector: 'app-footer',
    templateUrl: './footer.component.html',
    standalone: true,
    imports: [LogoComponent]
})
export class FooterComponent {
  constructor(public layoutService: LayoutService) {
  }
}
