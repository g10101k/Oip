import { Component, ElementRef } from '@angular/core';
import { LayoutService } from "oip/common";

@Component({
    selector: 'app-sidebar',
    templateUrl: './sidebar.component.html'
})
export class SidebarComponent {
    constructor(public layoutService: LayoutService, public el: ElementRef) { }
}
