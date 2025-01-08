import { Component } from '@angular/core';
import { ButtonModule } from "primeng/button";
import { RippleModule } from "primeng/ripple";
import { RouterLink } from "@angular/router";

@Component({
  selector: 'app-error',
  templateUrl: './error.component.html',
  standalone: true,
  imports: [
    ButtonModule,
    RippleModule,
    RouterLink
  ]
})
export class ErrorComponent {
}
