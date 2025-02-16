import { Component, inject, OnDestroy, OnInit, } from '@angular/core';
import { TopBarDto } from '../dtos/top-bar.dto'
import { TopBarService } from '../services/top-bar.service'
import { MsgService } from "../services/msg.service";
import { ActivatedRoute } from "@angular/router";
import { BaseDataService } from "../services/base-data.service";
import { TranslateService } from "@ngx-translate/core";

@Component({
  standalone: true,
  template: ''
})
export abstract class BaseComponent<TSettings> implements OnInit, OnDestroy {
  readonly topBarService: TopBarService = inject(TopBarService);
  readonly route: ActivatedRoute = inject(ActivatedRoute);
  readonly msgService = inject(MsgService);
  readonly baseDataService = inject(BaseDataService);
  readonly translateService = inject(TranslateService);

  /**
   * Feature settings
   */
  settings: TSettings = {} as TSettings;
  /**
   * Feature id
   */
  id: number;

  abstract controller: string;

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
    { id: 'content', icon: 'pi-box', caption: this.translateService.instant('baseComponent.content') },
    { id: 'settings', icon: 'pi-cog', caption: this.translateService.instant('baseComponent.settings') },
    { id: 'security', icon: 'pi-lock', caption: this.translateService.instant('baseComponent.security') },
  ];

  constructor() {
    // do nothing
  }

  async ngOnDestroy() {
    this.topBarService.setTopBarItems([]);
    this.topBarService.activeId = this.topBarItems[0].id;
  }

  async ngOnInit() {
    this.topBarService.setTopBarItems(this.topBarItems);
    this.topBarService.activeId = this.topBarItems[0].id;

    this.route.paramMap.subscribe(params => {
      this.id = +params.get('id');
    });
    await this.getSettings();
  }

  async getSettings() {
    await this.baseDataService.sendRequest<TSettings>(`${this.baseDataService.baseUrl}api/${this.controller}/get-module-instance-settings?id=${this.id}`).then(response => {
      this.settings = response;
    }).catch(error => {
      this.msgService.error(error);
    })
  }

  async saveSettings(settings: TSettings) {
    await this.baseDataService.sendRequest(`api/${this.controller}/put-module-instance-settings`, 'PUT', {
      id: this.id,
      settings: settings
    }).then(response => {
      this.msgService.success(this.translateService.instant('baseComponent.success'));
    }).catch(error => {
      this.msgService.error(error);
    });
  }
}
