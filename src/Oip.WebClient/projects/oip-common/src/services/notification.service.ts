import * as signalR from '@microsoft/signalr';
import { SecurityService } from './security.service';
import { inject, Injectable } from '@angular/core';
import { MsgService } from './msg.service';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private connection: signalR.HubConnection;
  private securityService = inject(SecurityService);
  private msgService = inject(MsgService);
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
      .withUrl('/hubs/notification', {
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

  private async startConnection() {
    try {
      await this.connection.start();
      console.log('SignalR Connected');
    } catch (err) {
      this.msgService.error(err.message);
      console.error('SignalR Connection Failed: ', err);
      setTimeout(() => this.startConnection(), 5000);
    }
  }
}
