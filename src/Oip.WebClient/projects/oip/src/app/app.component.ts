import { Component, inject, OnInit } from '@angular/core';
import { SecurityService } from "oip-common";
import { RouterOutlet } from '@angular/router';
import { ToastModule } from 'primeng/toast';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    standalone: true,
    imports: [ToastModule, RouterOutlet]
})
export class AppComponent implements OnInit {
  private readonly securityService = inject(SecurityService);

  constructor() {
  }

  ngOnInit() {
    this.securityService.auth();
  }
}
