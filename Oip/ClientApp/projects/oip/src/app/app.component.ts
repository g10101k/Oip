import {getManifest} from '@angular-architects/module-federation';
import {Component, OnInit} from '@angular/core';
import {Router} from '@angular/router';
import {CustomManifest, CustomRemoteConfig} from './utils/config';
import {buildRoutes} from './utils/routes';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {

  remotes: CustomRemoteConfig[] = [];

  constructor(
    private router: Router) {
  }

  async ngOnInit(): Promise<void> {

    const manifest = getManifest<CustomManifest>();
    const routes = buildRoutes(manifest);
    this.router.resetConfig(routes);
    this.remotes = Object.values(manifest);
  }
}

