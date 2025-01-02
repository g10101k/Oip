import { Component, OnInit } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { ButtonDirective, Button } from 'primeng/button';
import { Ripple } from 'primeng/ripple';
import { SplitButton } from 'primeng/splitbutton';

@Component({
    templateUrl: './buttondemo.component.html',
    imports: [ButtonDirective, Ripple, Button, SplitButton]
})
export class ButtonDemoComponent implements OnInit {

    items: MenuItem[] = [];

    loading = [false, false, false, false];

    ngOnInit() {
        this.items = [
            { label: 'Update', icon: 'pi pi-refresh' },
            { label: 'Delete', icon: 'pi pi-times' },
            { label: 'Angular.io', icon: 'pi pi-info', url: 'http://angular.io' },
            { separator: true },
            { label: 'Setup', icon: 'pi pi-cog' }
        ];
    }

    load(index: number) {
        this.loading[index] = true;
        setTimeout(() => this.loading[index] = false, 1000);
    }

}
