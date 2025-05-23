import { Component, inject, OnDestroy, OnInit, } from '@angular/core';
import { TopBarDto } from '../dtos/top-bar.dto'
import { TopBarService } from '../services/top-bar.service'
import { MsgService } from "../services/msg.service";
import { ActivatedRoute } from "@angular/router";
import { BaseDataService } from "../services/base-data.service";
import { TranslateService } from "@ngx-translate/core";
import { Title } from "@angular/platform-browser";

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
  readonly titleService = inject(Title);

  /**
   * Feature settings
   */
  settings: TSettings = {} as TSettings;
  /**
   * Feature id
   */
  id: number;

  /*
  * asp net controller without prefix 'api/'
  * */
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
    { id: 'content', icon: 'pi-box', caption: '' },
    { id: 'settings', icon: 'pi-cog', caption: '' },
    { id: 'security', icon: 'pi-lock', caption: '' },
  ];

  constructor() {

  }

  ngOnDestroy() {
    this.topBarService.setTopBarItems([]);
    this.topBarService.activeId = this.topBarItems[0].id;
  }

  async ngOnInit() {
    this.translateService.get('baseComponent.content').subscribe(value => this.topBarItems[0].caption = value);
    this.translateService.get('baseComponent.settings').subscribe(value => this.topBarItems[1].caption = value);
    this.translateService.get('baseComponent.security').subscribe(value => this.topBarItems[2].caption = value);

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
