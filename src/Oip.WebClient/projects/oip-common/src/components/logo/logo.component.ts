import { Component, input, linkedSignal } from '@angular/core';

@Component({
  selector: 'logo',
  standalone: true,
  template: `
    @if (!loadFailed() && src(); as url) {
      <img [src]="url" (error)="loadFailed.set(true)" alt="" />
    }
  `,
  styles: `
    :host {
      display: block;
      min-width: 0;
      max-width: min(100%, var(--logo-max-width, 100%));
    }

    :host(:empty) {
      display: none;
    }

    img {
      display: block;
      width: 100%;
      height: auto;
      max-height: var(--logo-max-height, none);
      object-fit: contain;
    }
  `
})
export class LogoComponent {
  readonly src = input<string | null>('/api/customization/get-logo');

  protected readonly loadFailed = linkedSignal({
    source: this.src,
    computation: () => false
  });
}
