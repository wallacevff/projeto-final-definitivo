import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiPagedResponse, normalizePagedResponse } from '../api/api.types';
import { ChatMessageCreatePayload, ChatMessageDto } from '../api/chat.api';

@Injectable({ providedIn: 'root' })
export class ChatMessagesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.baseUrl;

  getMessages(classGroupId: string, recipientId?: string | null, pageSize = 100) {
    const params: { [key: string]: string | number } = {
      ClassGroupId: classGroupId,
      PageNumber: 1,
      PageSize: pageSize
    };

    if (recipientId) {
      params['RecipientId'] = recipientId;
    }

    return this.http
      .get<ApiPagedResponse<ChatMessageDto>>(`${this.baseUrl}/chat/messages`, {
        params
      })
      .pipe(
        map(response => normalizePagedResponse(response).items),
        catchError(error => throwError(() => error))
      );
  }

  sendMessage(payload: ChatMessageCreatePayload) {
    return this.http
      .post<ChatMessageDto>(`${this.baseUrl}/chat/messages`, payload)
      .pipe(catchError(error => throwError(() => error)));
  }
}
