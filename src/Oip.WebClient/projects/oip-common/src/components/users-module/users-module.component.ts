import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { Avatar } from 'primeng/avatar';
import { Button } from 'primeng/button';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { SelectButton } from 'primeng/selectbutton';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { BaseModuleComponent } from '../base-module/base-module.component';
import { SecurityComponent } from '../security/security.component';
import { NoSettingsDto } from '../../dtos/no-settings.dto';
import { TopBarDto } from '../../dtos/top-bar.dto';
import { UsersModuleApi } from '../../api/users-module.api';
import { UserListItemDto, UsersSummaryDto, UsersSyncStatus } from '../../api/users-data-contracts';
import { provideTranslations } from '../../helpers/l10n.helper';
import en from './l10n/users-module.en.json';
import ru from './l10n/users-module.ru.json';

/** Status filter of the users list: all users, the active ones or the inactive ones. */
type ActivityFilter = 'all' | 'active' | 'inactive';

/** Row of the users table with the values the template shows. */
interface UserRow extends UserListItemDto {
  displayName: string;
  initials: string;
  avatarClass: string;
}

/**
 * Lists the users synchronized from Keycloak with server side paging, search and a status filter, shows how many of
 * them are active, and starts their full synchronization for users holding the edit right.
 */
@Component({
  selector: 'users-module',
  providers: [UsersModuleApi],
  imports: [
    SecurityComponent,
    TranslatePipe,
    FormsModule,
    Avatar,
    Button,
    IconField,
    InputIcon,
    InputText,
    Message,
    SelectButton,
    TableModule,
    Tag,
    Tooltip,
    DatePipe
  ],
  template: `
    @if (isContent) {
      <div class="card flex flex-col gap-5">
        <div class="flex flex-wrap items-center gap-4">
          <div
            class="flex items-center justify-center w-11 h-11 rounded-border bg-primary-50 text-primary-700 dark:bg-primary-950 dark:text-primary-200">
            <i class="pi pi-users text-xl"></i>
          </div>
          <div class="flex flex-col gap-1 grow basis-64">
            <h1 class="m-0 text-xl font-bold">{{ title }}</h1>
            <span class="text-muted-color">{{ 'users-module.subtitle' | translate }}</span>
          </div>
          <div class="flex flex-wrap items-center gap-2">
            <p-button
              icon="pi pi-refresh"
              qa-id="users-module-refresh-button"
              severity="secondary"
              tooltipPosition="bottom"
              [ariaLabel]="'users-module.refresh' | translate"
              [loading]="loading"
              [outlined]="true"
              [pTooltip]="'users-module.refresh' | translate"
              (onClick)="refresh()" />
            @if (canEdit) {
              <p-button
                icon="pi pi-sync"
                qa-id="users-module-sync-button"
                styleClass="whitespace-nowrap"
                [label]="'users-module.sync' | translate"
                [loading]="syncStatus.isRunning ?? false"
                (onClick)="syncAllUsers()" />
            }
          </div>
        </div>

        @if (syncStatus.isRunning) {
          <p-message icon="pi pi-spin pi-spinner" qa-id="users-module-sync-status" severity="info">
            <span class="font-bold">{{ 'users-module.syncRunning' | translate }}</span>
            <span class="ml-2">
              {{
                'users-module.syncRunningSince'
                  | translate: { date: (syncStatus.startedAt | date: layoutService.dateTimeFormat()) }
              }}
            </span>
          </p-message>
        } @else if (syncStatus.failed) {
          <p-message icon="pi pi-times-circle" qa-id="users-module-sync-status" severity="error">
            <span class="font-bold">{{ 'users-module.syncFailedTitle' | translate }}</span>
            <span class="ml-2">
              {{
                'users-module.syncFailedDetails'
                  | translate: { date: (syncStatus.finishedAt | date: layoutService.dateTimeFormat()) }
              }}
            </span>
          </p-message>
        } @else if (syncStatus.finishedAt) {
          <p-message icon="pi pi-check-circle" qa-id="users-module-sync-status" severity="success">
            <span class="font-bold">{{ 'users-module.syncFinished' | translate }}</span>
            <span class="ml-2">
              {{
                'users-module.syncFinishedDetails'
                  | translate
                    : {
                        date: (syncStatus.finishedAt | date: layoutService.dateTimeFormat()),
                        count: syncStatus.syncedCount ?? 0,
                        duration: syncDurationSeconds
                      }
              }}
            </span>
          </p-message>
        }

        <div class="grid grid-cols-3 gap-2 sm:gap-3" qa-id="users-module-summary">
          <div class="flex flex-col gap-1 border border-surface rounded-border px-3 py-2 sm:px-4 sm:py-3">
            <span class="text-sm sm:text-base text-muted-color truncate">{{
              'users-module.summary.total' | translate
            }}</span>
            <span class="text-xl sm:text-2xl font-bold">{{ summary.totalCount ?? 0 }}</span>
          </div>
          <div class="flex flex-col gap-1 border border-surface rounded-border px-3 py-2 sm:px-4 sm:py-3">
            <span class="text-sm sm:text-base text-muted-color truncate">{{
              'users-module.summary.active' | translate
            }}</span>
            <span class="text-xl sm:text-2xl font-bold text-green-700 dark:text-green-400">{{
              summary.activeCount ?? 0
            }}</span>
          </div>
          <div class="flex flex-col gap-1 border border-surface rounded-border px-3 py-2 sm:px-4 sm:py-3">
            <span class="text-sm sm:text-base text-muted-color truncate">{{
              'users-module.summary.inactive' | translate
            }}</span>
            <span class="text-xl sm:text-2xl font-bold text-muted-color">{{ summary.inactiveCount ?? 0 }}</span>
          </div>
        </div>

        <div class="flex flex-wrap items-center gap-3">
          <p-iconfield class="grow basis-72 max-w-96">
            <p-inputicon styleClass="pi pi-search" />
            <input
              class="w-full"
              pInputText
              qa-id="users-module-search-input"
              type="search"
              [attr.aria-label]="'users-module.searchPlaceholder' | translate"
              [placeholder]="'users-module.searchPlaceholder' | translate"
              [(ngModel)]="search"
              (ngModelChange)="onSearchChange()" />
          </p-iconfield>
          <p-selectbutton
            optionLabel="label"
            optionValue="value"
            qa-id="users-module-status-filter"
            [allowEmpty]="false"
            [ariaLabel]="'users-module.filter.label' | translate"
            [options]="activityOptions"
            [(ngModel)]="activity"
            (ngModelChange)="onActivityChange()" />
        </div>

        <p-table
          dataKey="userId"
          qa-id="users-module-table"
          [currentPageReportTemplate]="'users-module.pageReport' | translate"
          [first]="first"
          [lazy]="true"
          [loading]="loading"
          [paginator]="totalRecords > 0"
          [rows]="rows"
          [rowsPerPageOptions]="[20, 50, 100]"
          [showCurrentPageReport]="true"
          [tableStyle]="{ 'min-width': '40rem' }"
          [totalRecords]="totalRecords"
          [value]="users"
          (onLazyLoad)="onLazyLoad($event)">
          <ng-template pTemplate="header">
            <tr>
              <th scope="col">{{ 'users-module.columns.user' | translate }}</th>
              <th scope="col">{{ 'users-module.columns.status' | translate }}</th>
              <th scope="col">{{ 'users-module.columns.lastSyncedAt' | translate }}</th>
            </tr>
          </ng-template>
          <ng-template let-user pTemplate="body">
            <tr>
              <td>
                <div class="flex items-center gap-3">
                  <p-avatar class="shrink-0" shape="circle" [label]="user.initials" [styleClass]="user.avatarClass" />
                  <div class="flex flex-col min-w-0">
                    <span class="font-bold">{{ user.displayName }}</span>
                    <span class="text-muted-color">{{ user.email }}</span>
                  </div>
                </div>
              </td>
              <td>
                <p-tag
                  [severity]="user.isActive ? 'success' : 'secondary'"
                  [value]="(user.isActive ? 'users-module.active' : 'users-module.inactive') | translate" />
              </td>
              <td>
                <div class="flex flex-col">
                  <span>{{ user.lastSyncedAt | date: layoutService.dateTimeFormat() }}</span>
                  <span class="text-sm text-muted-color">{{ relativeTime(user.lastSyncedAt) }}</span>
                </div>
              </td>
            </tr>
          </ng-template>
          <ng-template pTemplate="emptymessage">
            <tr>
              <td class="py-8 text-center text-muted-color" colspan="3">
                {{ 'users-module.empty' | translate }}
              </td>
            </tr>
          </ng-template>
        </p-table>
      </div>
    } @else if (isSecurity) {
      <security [controller]="controller" [id]="id" />
    }
  `
})
export class UsersModuleComponent extends BaseModuleComponent<NoSettingsDto, NoSettingsDto> {
  private static readonly searchDelay = 300;
  private static readonly pollInterval = 2000;
  // Important: the avatar theme colors would win over plain utilities.
  private static readonly avatarClasses = [
    '!bg-red-100 !text-red-800 dark:!bg-red-900 dark:!text-red-100',
    '!bg-indigo-100 !text-indigo-800 dark:!bg-indigo-900 dark:!text-indigo-100',
    '!bg-green-100 !text-green-800 dark:!bg-green-900 dark:!text-green-100',
    '!bg-amber-100 !text-amber-800 dark:!bg-amber-900 dark:!text-amber-100',
    '!bg-sky-100 !text-sky-800 dark:!bg-sky-900 dark:!text-sky-100',
    '!bg-purple-100 !text-purple-800 dark:!bg-purple-900 dark:!text-purple-100'
  ];

  private readonly translations = provideTranslations({ en, ru });
  private readonly api = inject(UsersModuleApi);

  private searchTimer: ReturnType<typeof setTimeout> | null = null;
  private pollTimer: ReturnType<typeof setTimeout> | null = null;

  /** The module has no settings, so only the content and security tabs are shown. */
  public override topBarItems: TopBarDto[] = [
    { id: 'content', icon: 'pi-box', caption: '' },
    { id: 'security', icon: 'pi-lock', caption: '' }
  ];

  protected users: UserRow[] = [];
  protected summary: UsersSummaryDto = {};
  protected totalRecords = 0;
  protected first = 0;
  protected rows = 20;
  protected search = '';
  protected activity: ActivityFilter = 'all';
  protected activityOptions: { label: string; value: ActivityFilter }[] = [];
  protected loading = false;
  protected syncStatus: UsersSyncStatus = {};

  constructor() {
    super();
    this.translateService
      .stream('users-module.filter')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((filter: Record<ActivityFilter, string>) => {
        this.activityOptions = [
          { label: filter.all, value: 'all' },
          { label: filter.active, value: 'active' },
          { label: filter.inactive, value: 'inactive' }
        ];
      });
    this.destroyRef.onDestroy(() => {
      this.clearSearchTimer();
      this.stopPolling();
    });
  }

  /** Whole seconds the last finished synchronization took. */
  protected get syncDurationSeconds(): number {
    const { startedAt, finishedAt } = this.syncStatus;
    if (!startedAt || !finishedAt) {
      return 0;
    }

    return Math.max(0, Math.round((new Date(finishedAt).getTime() - new Date(startedAt).getTime()) / 1000));
  }

  /** The module has no settings to load. */
  override async getSettings(): Promise<void> {}

  protected override async onModuleInstanceChange(): Promise<void> {
    this.stopPolling();
    this.first = 0;
    await Promise.all([this.loadUsers(), this.loadSummary(), this.loadSyncStatus()]);
    if (this.syncStatus.isRunning) {
      this.schedulePoll();
    }
  }

  protected onLazyLoad(event: TableLazyLoadEvent): void {
    this.first = event.first ?? 0;
    this.rows = event.rows ?? this.rows;
    void this.loadUsers();
  }

  protected onSearchChange(): void {
    this.clearSearchTimer();
    this.searchTimer = setTimeout(() => {
      this.first = 0;
      void this.loadUsers();
    }, UsersModuleComponent.searchDelay);
  }

  protected onActivityChange(): void {
    this.first = 0;
    void this.loadUsers();
  }

  protected refresh(): void {
    void Promise.all([this.loadUsers(), this.loadSummary()]);
  }

  /** Describes how long ago the date was, e.g. "2 d ago", in the language of the interface. */
  protected relativeTime(value: Date | string | null | undefined): string {
    if (!value) {
      return '';
    }

    const seconds = Math.max(0, Math.floor((Date.now() - new Date(value).getTime()) / 1000));
    const units: [string, number][] = [
      ['years', 31536000],
      ['months', 2592000],
      ['days', 86400],
      ['hours', 3600],
      ['minutes', 60]
    ];
    for (const [unit, size] of units) {
      if (seconds >= size) {
        return this.translateService.instant(`users-module.ago.${unit}`, { count: Math.floor(seconds / size) });
      }
    }

    return this.translateService.instant('users-module.ago.now');
  }

  async loadUsers(): Promise<void> {
    if (!this.canRead) {
      return;
    }

    this.loading = true;
    try {
      const page = await this.api.getUsersByFilter({
        skip: this.first,
        take: this.rows,
        search: this.search.trim() || null,
        isActive: this.activity === 'all' ? null : this.activity === 'active'
      });
      this.users = (page.users ?? []).map((user) => this.toRow(user));
      this.totalRecords = page.totalCount ?? 0;
    } catch (error) {
      this.msgService.error(error);
    } finally {
      this.loading = false;
    }
  }

  async syncAllUsers(): Promise<void> {
    try {
      this.syncStatus = await this.api.syncAllUsers();
      this.schedulePoll();
    } catch (error) {
      this.msgService.error(error);
      // The synchronization may have been started by someone else: follow it until it finishes.
      await this.loadSyncStatus();
      if (this.syncStatus.isRunning) {
        this.schedulePoll();
      }
    }
  }

  private toRow(user: UserListItemDto): UserRow {
    const displayName = [user.firstName, user.lastName].filter((part) => !!part).join(' ') || (user.email ?? '');
    const initials = displayName
      .split(/\s+/)
      .filter((part) => !!part)
      .slice(0, 2)
      .map((part) => part[0].toUpperCase())
      .join('');
    const avatarClasses = UsersModuleComponent.avatarClasses;
    return {
      ...user,
      displayName,
      initials,
      avatarClass: avatarClasses[(user.userId ?? 0) % avatarClasses.length]
    };
  }

  private async loadSummary(): Promise<void> {
    if (!this.canRead) {
      return;
    }

    try {
      this.summary = await this.api.getUsersSummary();
    } catch (error) {
      this.msgService.error(error);
    }
  }

  private async loadSyncStatus(): Promise<void> {
    try {
      this.syncStatus = await this.api.getSyncStatus();
    } catch (error) {
      this.msgService.error(error);
    }
  }

  private schedulePoll(): void {
    this.stopPolling();
    this.pollTimer = setTimeout(() => void this.pollSyncStatus(), UsersModuleComponent.pollInterval);
  }

  private async pollSyncStatus(): Promise<void> {
    this.pollTimer = null;
    try {
      this.syncStatus = await this.api.getSyncStatus();
    } catch (error) {
      this.msgService.error(error);
      return;
    }

    if (this.syncStatus.isRunning) {
      this.schedulePoll();
      return;
    }

    if (this.syncStatus.failed) {
      this.msgService.error(this.translateService.instant('users-module.syncFailed'));
      return;
    }

    this.msgService.success(
      this.translateService.instant('users-module.syncCompleted', { count: this.syncStatus.syncedCount ?? 0 })
    );
    await Promise.all([this.loadUsers(), this.loadSummary()]);
  }

  private stopPolling(): void {
    if (this.pollTimer) {
      clearTimeout(this.pollTimer);
      this.pollTimer = null;
    }
  }

  private clearSearchTimer(): void {
    if (this.searchTimer) {
      clearTimeout(this.searchTimer);
      this.searchTimer = null;
    }
  }
}
