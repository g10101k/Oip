import { Component, ElementRef } from '@angular/core';
import { LayoutService } from "../../services/app.layout.service";

@Component({
    selector: 'app-sidebar',
    templateUrl: './sidebar.component.html'
})
export class SidebarComponent {
    constructor(public layoutService: LayoutService, public el: ElementRef) { }
}
