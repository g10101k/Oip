import { Component, computed, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { StyleClassModule } from 'primeng/styleclass';
import { AppConfigurator } from './app.configurator';
import { LayoutService } from '../services/app.layout.service';

@Component({
  selector: 'app-floating-configurator',
  imports: [ButtonModule, StyleClassModule, AppConfigurator],
  template: `
    <div class="fixed flex gap-4 top-8 right-8">
      <p-button
        id="oip-app-floating-configurator-dark-theme-button"
        severity="secondary"
        type="button"
        [icon]="isDarkTheme() ? 'pi pi-moon' : 'pi pi-sun'"
        [rounded]="true"
        (onClick)="toggleDarkMode()"/>
      <div class="relative">
        <p-button
          enterActiveClass="animate-scalein"
          enterFromClass="hidden"
          icon="pi pi-palette"
          id="oip-app-floating-configurator-next-button"
          leaveActiveClass="animate-fadeout"
          leaveToClass="hidden"
          pStyleClass="@next"
          [rounded]="true"
          type="button"
          [hideOnOutsideClick]="true"/>
        <app-configurator/>
      </div>
    </div>
  `
})
export class AppFloatingConfigurator {
  LayoutService = inject(LayoutService);

  isDarkTheme = computed(() => this.LayoutService.layoutConfig().darkTheme);

  toggleDarkMode() {
    this.LayoutService.layoutConfig.update((state) => ({
      ...state,
      darkTheme: !state.darkTheme
    }));
  }
}
