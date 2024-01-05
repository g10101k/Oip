import { getManifest } from '@angular-architects/module-federation';
import { Component, OnInit } from '@angular/core';
import { CustomManifest } from 'shared-lib';

@Component({
  selector: 'module-config',
  templateUrl: './module-config.component.html'
})
export class ModuleConfigComponent {

  manifest = getManifest<CustomManifest>();
}
