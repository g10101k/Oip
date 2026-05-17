import { CommonModule } from '@angular/common';
import { Component, ViewChild, computed, inject } from '@angular/core';
import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { Popover } from 'primeng/popover';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { NotificationApi } from '../api/notification.api';
import { UserNotificationDto } from '../api/notification-data-contracts';
import { MsgService } from '../services/msg.service';
import { NotificationService } from '../services/notification.service';

@Component({
  selector: 'app-user-notifications',
  standalone: true,
  imports: [CommonModule, BadgeModule, ButtonModule, PaginatorModule, Popover, ProgressSpinnerModule, Tag, Tooltip, TranslatePipe],
  template: `
    <button
      class="layout-topbar-action relative"
      id="oip-app-topbar-notification-button"
      type="button"
      (click)="toggle($event)"
      [attr.aria-label]="'Notifications: ' + unreadNotificationCount()">
      <i class="pi pi-bell"></i>
      @if (unreadNotificationCount() > 0) {
        <p-badge
          class="absolute -top-1 -right-1"
          severity="danger"
          [badgeSize]="'small'"
          [value]="unreadNotificationBadge()" />
      }
    </button>

    <p-popover #popover styleClass="w-[min(92vw,34rem)]">
      <div class="flex items-center justify-between gap-3 mb-4">
        <div class="font-semibold text-lg">
          {{ 'userNotifications.title' | translate }}
        </div>
        <p-button
          icon="pi pi-refresh"
          rounded="true"
          severity="secondary"
          text="true"
          tooltipPosition="bottom"
          [disabled]="loading"
          [pTooltip]="'userNotifications.refresh' | translate"
          (onClick)="loadNotifications()"></p-button>
      </div>

      @if (loading && notifications.length === 0) {
        <div class="flex justify-center py-8">
          <p-progressSpinner style="w-8 h-8"></p-progressSpinner>
        </div>
      } @else if (notifications.length === 0) {
        <div class="text-center py-8 text-surface-500">
          {{ 'userNotifications.empty' | translate }}
        </div>
      } @else {
        <div class="flex flex-col gap-3 max-h-[26rem] overflow-y-auto pr-1">
          @for (notification of notifications; track notification.notificationUserId) {
            <div class="border border-surface-200 dark:border-surface-700 rounded-md p-3">
              <div class="flex items-start justify-between gap-3">
                <div class="min-w-0">
                  <div class="font-medium leading-snug break-words">
                    {{ notification.subject }}
                  </div>
                  <div class="text-xs text-surface-500 mt-1">
                    {{ notification.createdAt | date: 'dd.MM.yyyy HH:mm' }}
                  </div>
                </div>
                @if (notification.importance) {
                  <p-tag
                    class="shrink-0"
                    [severity]="getImportanceSeverity(notification.importance)"
                    [value]="notification.importance"></p-tag>
                }
              </div>

              <div class="text-sm text-surface-700 dark:text-surface-200 mt-3 whitespace-pre-line break-words">
                {{ notification.message }}
              </div>

              <div class="flex justify-end mt-3">
                <p-button
                  icon="pi pi-check"
                  size="small"
                  [label]="'userNotifications.markAsRead' | translate"
                  [loading]="readingNotificationId === notification.notificationUserId"
                  (onClick)="markAsRead(notification)"></p-button>
              </div>
            </div>
          }
        </div>

        <p-paginator
          styleClass="mt-3"
          [first]="skip"
          [rows]="take"
          [totalRecords]="totalCount"
          [rowsPerPageOptions]="[5, 10, 20]"
          (onPageChange)="onPageChange($event)"></p-paginator>
      }
    </p-popover>
  `
})
export class UserNotificationsComponent {
  @ViewChild('popover') private popover?: Popover;

  protected notifications: UserNotificationDto[] = [];
  protected totalCount = 0;
  protected skip = 0;
  protected take = 5;
  protected loading = false;
  protected readingNotificationId?: number;

  private readonly notificationApi = inject(NotificationApi);
  private readonly notificationService = inject(NotificationService);
  private readonly msgService = inject(MsgService);
  private readonly translateService = inject(TranslateService);

  protected readonly unreadNotificationCount = computed(() => this.notificationService.unreadNotificationCount() ?? 0);
  protected readonly unreadNotificationBadge = computed(() => {
    const count = this.unreadNotificationCount();

    return count > 99 ? '99+' : count.toString();
  });

  protected async toggle(event: Event): Promise<void> {
    this.popover?.toggle(event);
    await this.loadNotifications();
  }

  async loadNotifications(): Promise<void> {
    this.loading = true;

    try {
      const response = await this.notificationApi.getNotificationByUser({
        skip: this.skip,
        take: this.take,
        unreadOnly: true
      });

      this.notifications = response.notifications ?? [];
      this.totalCount = response.totalCount ?? 0;
    } catch (error) {
      this.msgService.error(error);
    } finally {
      this.loading = false;
    }
  }

  async onPageChange(event: PaginatorState): Promise<void> {
    this.skip = event.first ?? 0;
    this.take = event.rows ?? this.take;
    await this.loadNotifications();
  }

  async markAsRead(notification: UserNotificationDto): Promise<void> {
    if (!notification.notificationUserId) {
      return;
    }

    this.readingNotificationId = notification.notificationUserId;

    try {
      await this.notificationApi.markNotificationAsRead({ id: notification.notificationUserId });
      this.msgService.success(this.translateService.instant('userNotifications.markedAsRead'));
      this.notificationService.loadUnreadNotificationCount();

      if (this.notifications.length === 1 && this.skip > 0) {
        this.skip = Math.max(this.skip - this.take, 0);
      }

      await this.loadNotifications();
    } catch (error) {
      this.msgService.error(error);
    } finally {
      this.readingNotificationId = undefined;
    }
  }

  protected getImportanceSeverity(importance: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (importance) {
      case 'Critical':
      case 'High':
        return 'danger';
      case 'Medium':
        return 'warn';
      case 'Low':
        return 'info';
      default:
        return 'secondary';
    }
  }
}
