import { Component } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { Route, RouterLink } from '@angular/router';

@Component({
    selector: 'app-access',
    templateUrl: './access.component.html',
    imports: [ButtonDirective, RouterLink],
})
export class AccessComponent {
}
