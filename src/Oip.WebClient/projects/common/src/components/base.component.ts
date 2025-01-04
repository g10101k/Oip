import { Component, inject, OnDestroy, OnInit, } from '@angular/core';
import { TopBarDto } from '../dtos/top-bar.dto'
import { TopBarService } from '../services/top-bar.service'
import { MsgService } from "../services/msg.service";
import { ActivatedRoute } from "@angular/router";

@Component({
  standalone: true,
  template: ''
})
export class BaseComponent implements OnInit, OnDestroy {
  id: number;
  readonly topBarService: TopBarService = inject(TopBarService);
  readonly route: ActivatedRoute = inject(ActivatedRoute);
  readonly msgService = inject(MsgService);

  get isContent(): boolean {
    return this.topBarService.checkId('content');
  }

  get isSettings(): boolean {
    return this.topBarService.checkId('settings');
  }

  get isSecurity(): boolean {
    return this.topBarService.checkId('security');
  }

  public topBarItems: TopBarDto [] = [
    { id: 'content', icon: 'pi-box', caption: 'Content', },
    { id: 'settings', icon: 'pi-cog', caption: 'Settings', },
    { id: 'security', icon: 'pi-lock', caption: 'Security', },
  ];

  constructor() {
  }

  ngOnDestroy(): void {
    this.topBarService.setTopBarItems([]);
    this.topBarService.activeIndex = 0;
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.id = +params.get('id');
    });
    this.topBarService.setTopBarItems(this.topBarItems);
    this.topBarService.activeIndex = 0;
  }
}
