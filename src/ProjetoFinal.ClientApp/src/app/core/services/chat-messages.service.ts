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

  getMessages(classGroupId: string, pageSize = 100) {
    return this.http
      .get<ApiPagedResponse<ChatMessageDto>>(`${this.baseUrl}/chat/messages`, {
        params: { ClassGroupId: classGroupId, PageSize: pageSize }
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
