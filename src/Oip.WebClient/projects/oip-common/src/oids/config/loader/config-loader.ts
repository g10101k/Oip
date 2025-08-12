import { Provider } from '@angular/core';
import { forkJoin, Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { OpenIdConfiguration } from '../openid-configuration';

export class OpenIdConfigLoader {
  loader?: Provider;
}

export abstract class StsConfigLoader {
  abstract loadConfigs(): OpenIdConfiguration[];
}

export class StsConfigStaticLoader implements StsConfigLoader {
  constructor(private readonly passedConfigs: OpenIdConfiguration | OpenIdConfiguration[]) {
  }

  loadConfigs(): OpenIdConfiguration[] {
    if (Array.isArray(this.passedConfigs)) {
      return (this.passedConfigs);
    }
    return ([this.passedConfigs]);
  }
}

export class StsConfigHttpLoader implements StsConfigLoader {
  constructor(private readonly configs$: | OpenIdConfiguration | OpenIdConfiguration[]) {
  }

  loadConfigs(): OpenIdConfiguration[] {
    if (Array.isArray(this.configs$)) {
      return this.configs$ as OpenIdConfiguration[];
    }
    return [this.configs$] as OpenIdConfiguration[];
  }
}
