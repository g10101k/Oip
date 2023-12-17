import {getManifest} from '@angular-architects/module-federation';
import {Component, OnInit} from '@angular/core';
import {Router} from '@angular/router';
import {buildRoutes} from './utils/routes';
import {CustomManifest, CustomRemoteConfig} from "shared-lib";
import {AuthLibService} from "auth-lib";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {

  remotes: CustomRemoteConfig[] = [];
  title: string = "shell";

  constructor(private router: Router) {
  }

  async ngOnInit(): Promise<void> {
    const manifest = getManifest<CustomManifest>();
    const routes = buildRoutes(manifest);
    this.router.resetConfig(routes);
    this.remotes = Object.values(manifest);
  }
}

