import { Component, effect, inject, OnDestroy, OnInit, Signal, signal, WritableSignal, } from '@angular/core';
import { TopBarDto } from '../dtos/top-bar.dto'
import { TopBarService } from '../services/top-bar.service'
import { MsgService } from "../services/msg.service";
import { ActivatedRoute } from "@angular/router";
import { BaseDataService } from "../services/base-data.service";
import { TranslateService } from "@ngx-translate/core";
import { Title } from "@angular/platform-browser";
import { Subject, Subscription } from "rxjs";

@Component({ standalone: true, template: '' })
export abstract class BaseModuleComponent<TBackendStoreSettings, TLocalStoreSettings> implements OnInit, OnDestroy {
  /**
   * Provides access to topbar related functionality, such as managing visibility and content.
   * @type {TopBarService}
   */
  public readonly topBarService: TopBarService = inject(TopBarService);
  /**
   * Provides access to information about the current route.
   * This includes route parameters, data, and the route's path.
   */
  public readonly route: ActivatedRoute = inject(ActivatedRoute);
  /**
   * Provides access to messaging services.
   */
  public readonly msgService = inject(MsgService);
  /**
   * Provides access to base data service functionality.
   * @deprecated The method should not be used
   */
  public readonly baseDataService: BaseDataService = inject(BaseDataService);
  /**
   * Provides access to translation functionality.
   */
  public readonly translateService = inject(TranslateService);
  /**
   * Provides access to the application's title service.
   */
  public readonly titleService = inject(Title);
  /**
   * Represents a subscription to an observable.
   * Manages the lifecycle of receiving data from the observable and allows unsubscribing to stop receiving data.
   * @type {Subscription}
   */
  private subscriptions: Subscription[] = [];
  /**
   * Configuration object for backend storage
   * @type {TBackendStoreSettings}
   */
  public settings: TBackendStoreSettings = {} as TBackendStoreSettings;

  /**
   * Configuration object for local storage.
   * @type {TLocalStoreSettings}
   */
  public _localSettings: TLocalStoreSettings = {} as TLocalStoreSettings;
  /**
   * A signal representing the local application settings.  Changes to this signal
   * propagate updates to the underlying local store settings.
   * @type {WritableSignal<TLocalStoreSettings>}
   */
  public localSettings: WritableSignal<TLocalStoreSettings> = signal<TLocalStoreSettings>(this._localSettings);
  /**
   * A Subject emitting updates to local store settings. Observables can subscribe to this Subject to react to changes in local settings.
   * @type {Subject<TLocalStoreSettings>}
   */
  public localSettingsUpdate: Subject<TLocalStoreSettings> = new Subject<TLocalStoreSettings>();
  /**
   * The title of the component. Get from Title in ngOnInit
   * @type {string}
   */
  public title: string;

  /**
   * Updates local settings and persists them to local storage.
   * @return {void}
   */
  private onConfigUpdate(): void {
    if (this.localSettings() == {} as TLocalStoreSettings) {
      this._localSettings = { ...this.localSettings() };
      this.localSettingsUpdate.next(this._localSettings);
      localStorage.setItem(`Instance_${this.id}`, JSON.stringify(this._localSettings))
    }
  }

  private getLocalStorageSettings() {
    try {
      const localStorageSettingsString = localStorage.getItem(`Instance_${this.id}`);
      if (localStorageSettingsString != null) {
        this.localSettings.set(JSON.parse(localStorageSettingsString) as TLocalStoreSettings);
      }
    } catch (error) {
      this.msgService.error(error, "Error parsing layoutConfig:");
      this.localSettings.set({} as TLocalStoreSettings);
    }
  }

  /**
   * A unique numerical identifier for the module.
   * @type {number}
   */
  id: number | undefined = undefined;

  /**
   * The name of the controller currently handling the request.
   * @type {string}
   */
  controller: string;

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

  /**
   * Initializes the component and subscribes to local settings updates.
   */
  constructor() {
    effect(() => {
      const config = this.localSettings();
      if (config) {
        this.onConfigUpdate();
      }
    });
    this.subscriptions.push(this.route.url.subscribe(url => {
      this.controller = url[0].path;
    }));
    this.subscriptions.push(this.route.paramMap.subscribe(params => {
      this.id = +params.get('id');
      this.getLocalStorageSettings();
    }));
  }

  /**
   * Clears the top bar items and sets the active ID to the first item.
   */
  ngOnDestroy() {
    this.topBarService.setTopBarItems([]);
    this.topBarService.activeId = this.topBarItems[0].id;
    this.subscriptions.map(s => s.unsubscribe());
  }

  /**
   * Initializes the component. Subscribes to translation service to set captions for top bar items,
   * sets the top bar items via the top bar service, sets the active ID,
   * subscribes to route parameters to get the ID, and retrieves settings.
   * @return {Promise<void>} A promise that resolves when the initialization is complete.
   */
  async ngOnInit(): Promise<void> {
    this.translateService.get('baseComponent.content').subscribe(value => this.topBarItems[0].caption = value);
    this.translateService.get('baseComponent.settings').subscribe(value => this.topBarItems[1].caption = value);
    this.translateService.get('baseComponent.security').subscribe(value => this.topBarItems[2].caption = value);

    this.topBarService.setTopBarItems(this.topBarItems);
    this.topBarService.activeId = this.topBarItems[0].id;

    await this.getSettings();

    this.title = this.titleService.getTitle()
  }

  /**
   * Retrieves module instance settings.
   * @return {Promise<void>} A promise that resolves when settings are retrieved.
   */
  async getSettings(): Promise<void> {
    await this.baseDataService.sendRequest<TBackendStoreSettings>(`${this.baseDataService.baseUrl}api/${this.controller}/get-module-instance-settings?id=${this.id}`).then(response => {
      this.settings = response;
    }).catch(error => {
      this.msgService.error(error);
    })
  }

  /**
   * Saves the provided settings to the backend.
   * @param {TLocalStoreSettings} settings - The settings object to save.
   * @return {Promise<void>} A promise that resolves when the settings are saved. Reject if an error occurs.
   */
  async saveSettings(settings: TBackendStoreSettings): Promise<void> {
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
