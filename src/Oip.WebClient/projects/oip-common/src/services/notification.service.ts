import * as signalR from '@microsoft/signalr';
import { SecurityService } from './security.service';
import { inject, Injectable, signal } from '@angular/core';
import { MsgService } from './msg.service';
import { OIP_FRONTEND_CONFIG } from './frontend-config';
import { NotificationApi } from '../api/notification.api';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private connection: signalR.HubConnection;
  private securityService = inject(SecurityService);
  private msgService = inject(MsgService);
  private frontendConfig = inject(OIP_FRONTEND_CONFIG);
  private notificationApi = inject(NotificationApi);
  private securityData: string | null = null;
  unreadNotificationCount = signal(0);

  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this.resolveHubUrl(), {
        accessTokenFactory: () => this.securityData ?? '',
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .configureLogging(signalR.LogLevel.Error)
      .build();

    this.connection.on('ReceiveNotification', (notification) => {
      const opt = {
        severity: notification.severity,
        summary: notification.subject,
        detail: notification.message,
        life: 0
      };
      this.msgService.add(opt);
      this.unreadNotificationCount.update((count) => count + 1);
    });

    this.securityService.getAccessToken().subscribe((token) => {
      this.securityData = token;
      if (!token) {
        this.unreadNotificationCount.set(0);
        return;
      }

      this.loadUnreadNotificationCount();

      if (this.connection.state === signalR.HubConnectionState.Disconnected) {
        this.connection.start().catch((error) => console.error('Failed to start notification connection', error));
        return;
      }

      this.connection.stop().then(() => {
        this.connection.start().catch((error) => console.error('Failed to restart notification connection', error));
      });
    });
  }

  loadUnreadNotificationCount(): void {
    this.notificationApi
      .getNotificationCountByUser()
      .then((response) => this.unreadNotificationCount.set(response.count ?? 0))
      .catch((error) => console.error('Failed to load notification count', error));
  }

  private resolveHubUrl(): string {
    if (this.frontendConfig.notificationHubUrl) {
      return this.frontendConfig.notificationHubUrl;
    }

    if (this.frontendConfig.apiBaseUrl) {
      return `${this.frontendConfig.apiBaseUrl.replace(/\/$/, '')}/hubs/notification`;
    }

    return '/hubs/notification';
  }
}
