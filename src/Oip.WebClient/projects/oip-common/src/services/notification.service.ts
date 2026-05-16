import * as signalR from '@microsoft/signalr';
import { SecurityService } from './security.service';
import { inject, Injectable, signal } from '@angular/core';
import { MsgService } from './msg.service';
import { NotificationApi } from '../api/notification.api';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private connection: signalR.HubConnection;
  private securityService = inject(SecurityService);
  private msgService = inject(MsgService);
  private notificationApi = inject(NotificationApi);
  unreadNotificationCount = signal(0);

  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/notification', {
        withCredentials: true,
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

    this.securityService.isAuthenticated().subscribe((authenticated) => {
      if (!authenticated) {
        if (this.connection.state !== signalR.HubConnectionState.Disconnected) {
          this.connection.stop();
        }
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
}
