import { Component, OnInit } from '@angular/core';
import { MenuItem, PrimeTemplate } from 'primeng/api';
import { Toolbar } from 'primeng/toolbar';
import { ButtonDirective, Button } from 'primeng/button';
import { SplitButton } from 'primeng/splitbutton';
import { Accordion, AccordionTab } from 'primeng/accordion';
import { TabView, TabPanel } from 'primeng/tabview';
import { Panel } from 'primeng/panel';
import { Fieldset } from 'primeng/fieldset';
import { Menu } from 'primeng/menu';
import { InputText } from 'primeng/inputtext';
import { Divider } from 'primeng/divider';
import { Splitter } from 'primeng/splitter';

@Component({
    templateUrl: './panelsdemo.component.html',
    imports: [Toolbar, ButtonDirective, SplitButton, Accordion, AccordionTab, TabView, TabPanel, Panel, Fieldset, Menu, InputText, Button, Divider, Splitter, PrimeTemplate]
})
export class PanelsDemoComponent implements OnInit {

  items: MenuItem[] = [];

  cardMenu: MenuItem[] = [];

  ngOnInit() {
    this.items = [
      { label: 'Angular.io', icon: 'pi pi-external-link', url: 'http://angular.io' },
      { label: 'Theming', icon: 'pi pi-bookmark', routerLink: ['/theming'] }
    ];

    this.cardMenu = [
      {
        label: 'Save', icon: 'pi pi-fw pi-check'
      },
      {
        label: 'Update', icon: 'pi pi-fw pi-refresh'
      },
      {
        label: 'Delete', icon: 'pi pi-fw pi-trash'
      },
    ];
  }

}
