import { Component, inject, OnDestroy, OnInit, } from '@angular/core';
import { TopBarDto } from '../dtos/top-bar.dto'
import { TopBarService } from '../services/top-bar.service'
import { MsgService } from "../services/msg.service";
import { ActivatedRoute } from "@angular/router";
import { BaseDataService } from "../services/base-data.service";
import { TranslateService } from "@ngx-translate/core";
import { Title } from "@angular/platform-browser";

@Component({ standalone: true, template: '' })
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

  /**
   * Checks if the content ID is present.
   * @returns {boolean} True if the content ID is present, false otherwise.
   */
  get isContent(): boolean {
    return this.topBarService.checkId('content');
  }

  /**
   * Checks if the settings ID is present.
   * @returns {boolean} True if the settings ID is present, false otherwise.
   */
  get isSettings(): boolean {
    return this.topBarService.checkId('settings');
  }

  /**
   * Checks if the security ID is present.
   * @returns {boolean} True if the security ID is present, false otherwise.
   */
  get isSecurity(): boolean {
    return this.topBarService.checkId('security');
  }

  /**
   * Defines the top bar items.
   */
  public topBarItems: TopBarDto [] = [
    { id: 'content', icon: 'pi-box', caption: '' },
    { id: 'settings', icon: 'pi-cog', caption: '' },
    { id: 'security', icon: 'pi-lock', caption: '' },
  ];

  constructor() {

  }

  /**
   * Clears the top bar items and sets the active ID to the first item.
   */
  ngOnDestroy() {
    this.topBarService.setTopBarItems([]);
    this.topBarService.activeId = this.topBarItems[0].id;
  }

  /**
   * Initializes the component. Subscribes to translation service to set captions for top bar items,
   * sets the top bar items via the top bar service, sets the active ID,
   * subscribes to route parameters to get the ID, and retrieves settings.
   * @return {Promise<void>} A promise that resolves when the initialization is complete.
   */
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

  /**
   * Retrieves the settings for the module instance.
   */
  async getSettings() {
    await this.baseDataService.sendRequest<TSettings>(`${this.baseDataService.baseUrl}api/${this.controller}/get-module-instance-settings?id=${this.id}`).then(response => {
      this.settings = response;
    }).catch(error => {
      this.msgService.error(error);
    })
  }


  /**
   * Saves the settings for the module instance.
   * @param settings The settings to save.
   */
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
