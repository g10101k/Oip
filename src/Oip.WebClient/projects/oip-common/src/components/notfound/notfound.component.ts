import { Component } from '@angular/core';
import { RouterLink } from "@angular/router";
import { LogoComponent } from "../logo/logo.component";

@Component({
  selector: 'app-notfound',
  templateUrl: './notfound.component.html',
  imports: [
    RouterLink,
    LogoComponent
  ],
  standalone: true
})
export class NotfoundComponent { }
