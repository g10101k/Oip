import { Component, Input } from '@angular/core';

@Component({
  selector: 'logo',
  standalone: true,
  templateUrl: './logo.component.html',
})
export class LogoComponent {
  @Input() width: number;
  @Input() height: number;
}
