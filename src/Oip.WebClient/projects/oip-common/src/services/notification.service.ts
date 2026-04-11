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
  private securityData: string;

  constructor() {
    this.securityService.getAccessToken().subscribe((token) => {
      this.securityData = token;
      if (token) {
        this.connection.stop().then(() => {
          this.connection.start().then();
        });
      }
    });
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this.resolveHubUrl(), {
        accessTokenFactory: () => this.securityData,
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
