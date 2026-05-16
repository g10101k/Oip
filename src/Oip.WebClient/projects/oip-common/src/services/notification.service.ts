import * as signalR from '@microsoft/signalr';
import { SecurityService } from './security.service';
import { inject, Injectable } from '@angular/core';
import { MsgService } from './msg.service';
import { OIP_FRONTEND_CONFIG } from './frontend-config';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private connection: signalR.HubConnection;
  private securityService = inject(SecurityService);
  private msgService = inject(MsgService);
  private frontendConfig = inject(OIP_FRONTEND_CONFIG);

  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this.resolveHubUrl(), {
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
    });

    this.securityService.isAuthenticated().subscribe((authenticated) => {
      if (!authenticated) {
        if (this.connection.state !== signalR.HubConnectionState.Disconnected) {
          this.connection.stop();
        }
        return;
      }

      if (this.connection.state === signalR.HubConnectionState.Disconnected) {
        this.connection.start().catch((error) => console.error('Failed to start notification connection', error));
        return;
      }

      this.connection.stop().then(() => {
        this.connection.start().catch((error) => console.error('Failed to restart notification connection', error));
      });
    });
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
