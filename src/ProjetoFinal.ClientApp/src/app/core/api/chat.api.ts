export interface ChatMessageDto {
  Id: string;
  ClassGroupId: string;
  SenderId: string;
  SenderName: string;
  RecipientId?: string;
  RecipientName?: string;
  ReplyToMessageId?: string;
  MediaResourceId?: string;
  Message: string;
  SentAt: string;
  UpdatedAt?: string;
  IsSystemMessage: boolean;
  IsDirectMessage: boolean;
}

export interface ChatMessageCreatePayload {
  ClassGroupId: string;
  SenderId: string;
  RecipientId?: string;
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

export interface ChatParticipant {
  userId: string;
  userName: string;
  roleLabel: string;
}
