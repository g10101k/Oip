import { getManifest } from '@angular-architects/module-federation';
import { Component, OnInit } from '@angular/core';
import { PrimeNGConfig } from 'primeng/api';
import { Router } from '@angular/router';
import { buildRoutes } from './utils/routes';
import { CustomManifest, CustomRemoteConfig } from "shared-lib";
import { AuthLibService } from "auth-lib";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {

  remotes: CustomRemoteConfig[] = [];
  title: string = "shell";

  constructor(private router: Router, private primengConfig: PrimeNGConfig) {
  }

  ngOnInit() {
    const manifest = getManifest<CustomManifest>();
    const routes = buildRoutes(manifest);
    this.router.resetConfig(routes);
    this.remotes = Object.values(manifest);
    this.primengConfig.ripple = true;
  }
}
