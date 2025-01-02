import { Component } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'app-error',
    templateUrl: './error.component.html',
    imports: [ButtonDirective, RouterLink],
})
export class ErrorComponent {
}
