import { Injectable, Type, InjectionToken, inject } from '@angular/core';
import { LogoComponent } from '../components/logo.component';
import { Provider } from '@angular/core';

export const LOGO_COMPONENT_TOKEN = new InjectionToken<Type<any>>('LOGO_COMPONENT_TOKEN');

export function provideLogoComponent(component: Type<any>): Provider {
  return {
    provide: LOGO_COMPONENT_TOKEN,
    useValue: component
  };
}

@Injectable({ providedIn: 'root' })
export class LogoService {
  private customLogoComponent = inject(LOGO_COMPONENT_TOKEN, { optional: true });

  getLogoComponent(): Type<any> {
    return this.customLogoComponent || LogoComponent;
  }
}
