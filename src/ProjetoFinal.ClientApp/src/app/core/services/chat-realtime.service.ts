import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';

import { environment } from '../../../environments/environment';
import { ChatMessageDto, ChatPresenceUserDto } from '../api/chat.api';
import { AuthService } from './auth.service';

interface ChatRealtimeHandlers {
  onMessageReceived: (message: ChatMessageDto) => void;
  onMessageUpdated?: (message: ChatMessageDto) => void;
  onMessageDeleted?: (messageId: string) => void;
  onPresenceSnapshot?: (users: ChatPresenceUserDto[]) => void;
}

@Injectable({ providedIn: 'root' })
export class ChatRealtimeService {
  private readonly authService = inject(AuthService);
  private connection: signalR.HubConnection | null = null;
  private activeClassGroupId: string | null = null;

  async connectToClassGroup(classGroupId: string, handlers: ChatRealtimeHandlers): Promise<void> {
    if (!classGroupId) {
      return;
    }

    if (this.activeClassGroupId === classGroupId && this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    await this.disconnect();

    const hubUrl = `${this.resolveBaseUrl()}/hubs/chat`;
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => this.authService.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.connection.on('MessageReceived', handlers.onMessageReceived);
    if (handlers.onMessageUpdated) {
      this.connection.on('MessageUpdated', handlers.onMessageUpdated);
    }
    if (handlers.onMessageDeleted) {
      this.connection.on('MessageDeleted', handlers.onMessageDeleted);
    }
    if (handlers.onPresenceSnapshot) {
      this.connection.on('PresenceSnapshot', handlers.onPresenceSnapshot);
    }

    await this.connection.start();
    await this.connection.invoke('JoinClassGroup', classGroupId);
    this.activeClassGroupId = classGroupId;
  }

  async disconnect(): Promise<void> {
    if (!this.connection) {
      return;
    }

    const classGroupId = this.activeClassGroupId;
    const connection = this.connection;
    this.connection = null;
    this.activeClassGroupId = null;

    try {
      if (classGroupId && connection.state === signalR.HubConnectionState.Connected) {
        await connection.invoke('LeaveClassGroup', classGroupId);
      }
    } catch {
      // Ignora erros durante fechamento/reconexao.
    }

    await connection.stop();
  }

  private resolveBaseUrl(): string {
    return environment.baseUrl.replace(/\/api(?:\/v1)?$/, '');
  }
}
