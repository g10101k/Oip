import { Component, inject, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { ToolbarModule } from 'primeng/toolbar';
import { Tooltip } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { InputTextModule } from 'primeng/inputtext';
import { TranslatePipe } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';
import { MsgService } from '../../services/msg.service';
import { L10nService } from '../../services/l10n.service';
import { AppTitleService } from '../../services/app-title.service';
import { LayoutService } from '../../services/app.layout.service';
import { AuthSessionsApi } from '../../api/auth-sessions.api';
import { AuthSessionDto } from '../../api/data-contracts';

import en from './l10n/auth-sessions.en.json';
import ru from './l10n/auth-sessions.ru.json';

L10nService.registerTranslations({en, ru});

/**
 * Administrator page listing active authentication sessions with the ability to terminate them.
 */
@Component({
  imports: [
    FormsModule,
    TableModule,
    Tag,
    ButtonModule,
    ToolbarModule,
    Tooltip,
    ConfirmDialog,
    TranslatePipe,
    InputTextModule,
    DatePipe
  ],
  providers: [ConfirmationService, AuthSessionsApi],
  selector: 'app-auth-sessions',
  template: `
    <p-confirmDialog></p-confirmDialog>
    <div class="flex flex-col md:flex-row gap-4">
      <div class="card w-full">
        <div class="mb-4">
          <p-toolbar>
            <div class="flex flex-col md:flex-row md:items-center gap-2 w-full">
              <div class="font-semibold text-lg flex items-center gap-2 mx-2">
                <i class="pi pi-users"></i>
                {{ 'auth-sessions.title' | translate }}
              </div>

              <div class="flex-1"></div>
              <div class="flex items-center gap-1 w-full md:w-auto">
                <p-button
                  icon="pi pi-refresh"
                  rounded="true"
                  severity="secondary"
                  text="true"
                  tooltipPosition="bottom"
                  [loading]="loading"
                  [pTooltip]="'auth-sessions.refreshTooltip' | translate"
                  (onClick)="refreshAction()"></p-button>
                <p-button
                  icon="pi pi-power-off"
                  rounded="true"
                  severity="danger"
                  text="true"
                  tooltipPosition="bottom"
                  id="oip-auth-sessions-terminate-all"
                  [disabled]="loading || !hasOtherSessionsThanCurrent"
                  [pTooltip]="'auth-sessions.terminateAllTooltip' | translate"
                  (onClick)="terminateAllSessions()"></p-button>
                <input
                  class="w-full md:w-96"
                  pInputText
                  type="text"
                  id="oip-auth-sessions-filter"
                  [placeholder]="'auth-sessions.filterPlaceholder' | translate"
                  [(ngModel)]="userFilter"/>
                <p-button
                  icon="pi pi-filter-slash"
                  rounded="true"
                  severity="secondary"
                  text="true"
                  tooltipPosition="bottom"
                  id="oip-auth-sessions-clear-filter"
                  [disabled]="!userFilter"
                  [pTooltip]="'auth-sessions.clearFilterTooltip' | translate"
                  (onClick)="userFilter = ''"></p-button>
              </div>
            </div>
          </p-toolbar>
        </div>
        <p-table
          class="mt-4"
          dataKey="sessionId"
          [paginator]="true"
          [rows]="50"
          sortField="lastActivityUtc"
          [sortOrder]="-1"
          [value]="filteredSessions"
          [loading]="loading">
          <ng-template pTemplate="header">
            <tr>
              <th pSortableColumn="userName">
                {{ 'auth-sessions.table.user' | translate }}
                <p-sortIcon field="userName"></p-sortIcon>
              </th>
              <th pSortableColumn="createdUtc">
                {{ 'auth-sessions.table.created' | translate }}
                <p-sortIcon field="createdUtc"></p-sortIcon>
              </th>
              <th pSortableColumn="expiresUtc">
                {{ 'auth-sessions.table.expires' | translate }}
                <p-sortIcon field="expiresUtc"></p-sortIcon>
              </th>
              <th pSortableColumn="lastActivityUtc">
                {{ 'auth-sessions.table.lastActivity' | translate }}
                <p-sortIcon field="lastActivityUtc"></p-sortIcon>
              </th>
              <th pSortableColumn="ipAddress">
                {{ 'auth-sessions.table.ipAddress' | translate }}
                <p-sortIcon field="ipAddress"></p-sortIcon>
              </th>
              <th pSortableColumn="userAgent">
                {{ 'auth-sessions.table.userAgent' | translate }}
                <p-sortIcon field="userAgent"></p-sortIcon>
              </th>
              <th style="width: 7rem"></th>
            </tr>
          </ng-template>
          <ng-template let-session pTemplate="body">
            <tr>
              <td>
                <div class="flex items-center gap-2">
                  <div class="flex flex-col">
                    <span class="font-medium">{{ session.userName }}</span>
                    @if (session.displayName || session.email) {
                      <span class="text-sm text-muted-color">{{ session.displayName || session.email }}</span>
                    }
                  </div>
                  @if (session.isCurrent) {
                    <p-tag severity="info" [value]="'auth-sessions.table.current' | translate"></p-tag>
                  }
                </div>
              </td>
              <td>{{ session.createdUtc | date: layoutService.dateTimeFormat() }}</td>
              <td>{{ session.expiresUtc | date: layoutService.dateTimeFormat() }}</td>
              <td>{{ session.lastActivityUtc | date: layoutService.dateTimeFormat() }}</td>
              <td>{{ session.ipAddress }}</td>
              <td class="max-w-80 truncate" [pTooltip]="session.userAgent" tooltipPosition="top">
                {{ session.userAgent }}
              </td>
              <td>
                <div class="flex gap-1">
                  <p-button
                    icon="pi pi-sign-out"
                    rounded="true"
                    severity="danger"
                    text="true"
                    tooltipPosition="bottom"
                    [disabled]="session.isCurrent"
                    [pTooltip]="'auth-sessions.table.terminateTooltip' | translate"
                    (onClick)="terminateSession(session)"></p-button>
                  <p-button
                    icon="pi pi-user-minus"
                    rounded="true"
                    severity="danger"
                    text="true"
                    tooltipPosition="bottom"
                    [disabled]="!session.userId || !hasOtherSessions(session)"
                    [pTooltip]="'auth-sessions.table.terminateAllTooltip' | translate"
                    (onClick)="terminateUserSessions(session)"></p-button>
                </div>
              </td>
            </tr>
          </ng-template>
          <ng-template pTemplate="emptymessage">
            <tr>
              <td colspan="7">{{ 'auth-sessions.table.empty' | translate }}</td>
            </tr>
          </ng-template>
        </p-table>
      </div>
    </div>
  `
})
export class AuthSessionsComponent implements OnInit {
  protected sessions: AuthSessionDto[] = [];
  protected userFilter = '';
  protected loading = false;
  protected msgService = inject(MsgService);
  protected confirmationService = inject(ConfirmationService);
  protected l10nService = inject(L10nService);
  protected titleService = inject(AppTitleService);
  protected layoutService = inject(LayoutService);
  private authSessionsApi = inject(AuthSessionsApi);
  private readonly translationsReady: Promise<unknown>;

  constructor() {
    this.translationsReady = firstValueFrom(this.l10nService.loadComponentTranslations('auth-sessions'));
  }

  async ngOnInit() {
    await this.translationsReady;
    this.titleService.setTitle(this.t('auth-sessions.title'));
    await this.refreshAction();
  }

  protected get filteredSessions(): AuthSessionDto[] {
    const filter = this.userFilter.trim().toLowerCase();
    if (!filter) {
      return this.sessions;
    }
    return this.sessions.filter((session) =>
      [session.userName, session.displayName, session.email, session.userId, session.ipAddress].some((value) =>
        value?.toLowerCase().includes(filter)
      )
    );
  }

  async refreshAction() {
    this.loading = true;
    try {
      this.sessions = await this.authSessionsApi.getAuthSessions({});
    } catch (error) {
      this.msgService.errorFromException(
        error,
        this.t('auth-sessions.messages.loadError'),
        this.t('auth-sessions.messages.loadError')
      );
    } finally {
      this.loading = false;
    }
  }

  protected get hasOtherSessionsThanCurrent(): boolean {
    return this.sessions.some((x) => !x.isCurrent);
  }

  async terminateAllSessions() {
    await this.translationsReady;

    this.confirmationService.confirm({
      header: this.t('auth-sessions.confirm.header'),
      message: this.t('auth-sessions.confirm.terminateEveryoneMessage'),
      icon: 'pi pi-power-off',
      rejectButtonProps: {
        label: this.t('auth-sessions.confirm.cancel'),
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: this.t('auth-sessions.confirm.terminate'),
        severity: 'danger'
      },
      accept: async () => {
        try {
          const result = await this.authSessionsApi.deleteAllAuthSessions();
          await this.refreshAction();
          this.msgService.success(
            this.l10nService.instant('auth-sessions.messages.terminateAllSuccess', {
              count: result.deletedCount ?? 0
            })
          );
        } catch (error) {
          this.msgService.errorFromException(
            error,
            this.t('auth-sessions.messages.terminateError'),
            this.t('auth-sessions.messages.terminateError')
          );
        }
      }
    });
  }

  protected hasOtherSessions(session: AuthSessionDto): boolean {
    return this.sessions.some((x) => x.userId === session.userId && !x.isCurrent);
  }

  async terminateSession(session: AuthSessionDto) {
    await this.translationsReady;

    this.confirmationService.confirm({
      header: this.t('auth-sessions.confirm.header'),
      message: this.l10nService.instant('auth-sessions.confirm.terminateMessage', {
        user: session.userName ?? session.userId ?? ''
      }),
      icon: 'pi pi-sign-out',
      rejectButtonProps: {
        label: this.t('auth-sessions.confirm.cancel'),
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: this.t('auth-sessions.confirm.terminate'),
        severity: 'danger'
      },
      accept: async () => {
        try {
          await this.authSessionsApi.deleteAuthSession({sessionId: session.sessionId!});
          await this.refreshAction();
          this.msgService.success(this.t('auth-sessions.messages.terminateSuccess'));
        } catch (error) {
          this.msgService.errorFromException(
            error,
            this.t('auth-sessions.messages.terminateError'),
            this.t('auth-sessions.messages.terminateError')
          );
        }
      }
    });
  }

  async terminateUserSessions(session: AuthSessionDto) {
    await this.translationsReady;

    this.confirmationService.confirm({
      header: this.t('auth-sessions.confirm.header'),
      message: this.l10nService.instant('auth-sessions.confirm.terminateAllMessage', {
        user: session.userName ?? session.userId ?? ''
      }),
      icon: 'pi pi-user-minus',
      rejectButtonProps: {
        label: this.t('auth-sessions.confirm.cancel'),
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: this.t('auth-sessions.confirm.terminate'),
        severity: 'danger'
      },
      accept: async () => {
        try {
          const result = await this.authSessionsApi.deleteAuthSessionsByUser({userId: session.userId!});
          await this.refreshAction();
          this.msgService.success(
            this.l10nService.instant('auth-sessions.messages.terminateAllSuccess', {
              count: result.deletedCount ?? 0
            })
          );
        } catch (error) {
          this.msgService.errorFromException(
            error,
            this.t('auth-sessions.messages.terminateError'),
            this.t('auth-sessions.messages.terminateError')
          );
        }
      }
    });
  }

  t(key: string) {
    return this.l10nService.instant(key);
  }
}
