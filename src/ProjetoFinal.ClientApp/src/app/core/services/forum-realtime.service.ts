import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';

import { environment } from '../../../environments/environment';
import { ForumPostDto } from '../api/forum.api';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class ForumRealtimeService {
  private readonly authService = inject(AuthService);
  private connection: signalR.HubConnection | null = null;
  private activeThreadId: string | null = null;

  async connectToThread(threadId: string, onPostCreated: (post: ForumPostDto) => void): Promise<void> {
    if (!threadId) {
      return;
    }

    if (this.activeThreadId === threadId && this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    await this.disconnect();

    const hubUrl = `${this.resolveBaseUrl()}/hubs/forum`;
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => this.authService.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.connection.on('PostCreated', onPostCreated);

    await this.connection.start();
    await this.connection.invoke('JoinThread', threadId);
    this.activeThreadId = threadId;
  }

  async disconnect(): Promise<void> {
    if (!this.connection) {
      return;
    }

    const threadId = this.activeThreadId;
    const connection = this.connection;
    this.connection = null;
    this.activeThreadId = null;

    try {
      if (threadId && connection.state === signalR.HubConnectionState.Connected) {
        await connection.invoke('LeaveThread', threadId);
      }
    } catch {
      // Ignore leave failures during shutdown/reconnect.
    }

    await connection.stop();
  }

  private resolveBaseUrl(): string {
    return environment.baseUrl.replace(/\/api(?:\/v1)?$/, '');
  }
}
