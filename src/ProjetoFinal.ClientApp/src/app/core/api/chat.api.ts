export interface ChatMessageDto {
  Id: string;
  ClassGroupId: string;
  SenderId: string;
  SenderName: string;
  ReplyToMessageId?: string;
  MediaResourceId?: string;
  Message: string;
  SentAt: string;
  UpdatedAt?: string;
  IsSystemMessage: boolean;
}

export interface ChatMessageCreatePayload {
  ClassGroupId: string;
  SenderId: string;
  ReplyToMessageId?: string;
  MediaResourceId?: string;
  Message: string;
}

export interface ChatMessageUpdatePayload {
  Message: string;
  MediaResourceId?: string;
}

export interface ChatPresenceUserDto {
  UserId: string;
  UserName: string;
}
