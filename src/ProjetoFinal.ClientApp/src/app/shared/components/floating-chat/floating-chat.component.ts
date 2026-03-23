import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, ElementRef, computed, effect, inject, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { ChatMessageDto, ChatParticipant, ChatPresenceUserDto } from '../../../core/api/chat.api';
import { ClassEnrollmentDto, CourseDto } from '../../../core/api/courses.api';
import { AuthService } from '../../../core/services/auth.service';
import { ChatMessagesService } from '../../../core/services/chat-messages.service';
import { ChatRealtimeService } from '../../../core/services/chat-realtime.service';
import { CoursesService } from '../../../core/services/courses.service';

interface ChatRoom {
  classGroupId: string;
  classGroupName: string;
  courseTitle: string;
  participants: ChatParticipant[];
}

interface ChatConversation {
  type: 'group' | 'direct';
  participant?: ChatParticipant;
}

@Component({
  selector: 'app-floating-chat',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './floating-chat.component.html',
  styleUrl: './floating-chat.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FloatingChatComponent {
  private readonly authService = inject(AuthService);
  private readonly coursesService = inject(CoursesService);
  private readonly chatMessagesService = inject(ChatMessagesService);
  private readonly chatRealtimeService = inject(ChatRealtimeService);
  private readonly toastr = inject(ToastrService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);
  private readonly messagesContainer = viewChild<ElementRef<HTMLDivElement>>('messagesContainer');
  private readonly messagesBottomAnchor = viewChild<ElementRef<HTMLDivElement>>('messagesBottomAnchor');

  readonly currentUser = this.authService.currentUser;
  readonly isVisible = computed(() => !!this.currentUser());
  readonly isOpen = signal(false);
  readonly loadingRooms = signal(false);
  readonly loadingMessages = signal(false);
  readonly sendingMessage = signal(false);
  readonly rooms = signal<ChatRoom[]>([]);
  readonly selectedClassGroupId = signal<string | null>(null);
  readonly selectedParticipantId = signal<string | null>(null);
  readonly messages = signal<ChatMessageDto[]>([]);
  readonly onlineUsers = signal<ChatPresenceUserDto[]>([]);

  readonly selectedRoom = computed(() => {
    const currentId = this.selectedClassGroupId();
    return this.rooms().find(room => room.classGroupId === currentId) ?? null;
  });

  readonly selectedConversation = computed<ChatConversation>(() => {
    const room = this.selectedRoom();
    const participantId = this.selectedParticipantId();
    const participant = room?.participants.find(item => item.userId === participantId);
    return participant ? { type: 'direct', participant } : { type: 'group' };
  });

  readonly orderedMessages = computed(() => this.sortMessages(this.messages()));

  readonly selectedConversationTitle = computed(() => {
    const conversation = this.selectedConversation();
    return conversation.type === 'group'
      ? 'Canal da turma'
      : `Conversa com ${conversation.participant?.userName || 'o aluno'}`;
  });

  readonly selectedConversationSubtitle = computed(() => {
    const conversation = this.selectedConversation();
    if (conversation.type === 'group') {
      return 'Mensagem visivel para todos os participantes da turma.';
    }

    const status = conversation.participant && this.isParticipantOnline(conversation.participant.userId)
      ? 'online'
      : 'offline';
    return `${conversation.participant?.roleLabel || 'Participante'} • ${status}`;
  });

  readonly onlineUsersLabel = computed(() => this.onlineUsers().map(user => user.UserName).join(', '));

  readonly form = this.fb.group({
    message: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(2000)])
  });

  constructor() {
    this.destroyRef.onDestroy(() => {
      void this.chatRealtimeService.disconnect();
    });

    effect(() => {
      const user = this.currentUser();
      if (!user) {
        this.rooms.set([]);
        this.messages.set([]);
        this.onlineUsers.set([]);
        this.selectedClassGroupId.set(null);
        this.selectedParticipantId.set(null);
        void this.chatRealtimeService.disconnect();
        return;
      }

      this.loadRooms();
    });
  }

  toggleOpen(): void {
    if (!this.rooms().length && !this.loadingRooms()) {
      this.loadRooms();
    }

    this.isOpen.update(current => !current);
  }

  isOwnMessage(message: ChatMessageDto): boolean {
    return message.SenderId === this.currentUser()?.id;
  }

  isParticipantOnline(userId: string): boolean {
    return this.onlineUsers().some(user => user.UserId === userId);
  }

  selectRoom(room: ChatRoom): void {
    if (this.selectedClassGroupId() === room.classGroupId) {
      return;
    }

    this.selectedClassGroupId.set(room.classGroupId);
    this.selectedParticipantId.set(null);
    this.loadMessages(room.classGroupId, null);
    void this.connectRealtime(room.classGroupId);
  }

  selectGroupConversation(): void {
    const classGroupId = this.selectedClassGroupId();
    if (!classGroupId) {
      return;
    }

    this.selectedParticipantId.set(null);
    this.loadMessages(classGroupId, null);
  }

  selectDirectConversation(participant: ChatParticipant): void {
    const classGroupId = this.selectedClassGroupId();
    if (!classGroupId || this.selectedParticipantId() === participant.userId) {
      return;
    }

    this.selectedParticipantId.set(participant.userId);
    this.loadMessages(classGroupId, participant.userId);
  }

  sendMessage(): void {
    const room = this.selectedRoom();
    const user = this.currentUser();
    if (!room || !user) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const message = this.form.controls.message.value.trim();
    if (!message) {
      this.form.controls.message.setErrors({ required: true });
      return;
    }

    this.sendingMessage.set(true);
    this.chatMessagesService
      .sendMessage({
        ClassGroupId: room.classGroupId,
        SenderId: user.id,
        RecipientId: this.selectedParticipantId() ?? undefined,
        Message: message
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: chatMessage => {
          this.upsertMessage(chatMessage);
          this.form.reset({ message: '' });
          this.sendingMessage.set(false);
        },
        error: () => {
          this.toastr.error('Nao foi possivel enviar a mensagem do chat.');
          this.sendingMessage.set(false);
        }
      });
  }

  trackByRoom(_: number, room: ChatRoom): string {
    return room.classGroupId;
  }

  trackByParticipant(_: number, participant: ChatParticipant): string {
    return participant.userId;
  }

  trackByMessage(_: number, message: ChatMessageDto): string {
    return message.Id;
  }

  private loadRooms(): void {
    const user = this.currentUser();
    if (!user) {
      return;
    }

    this.loadingRooms.set(true);
    const request =
      user.role === 2
        ? this.coursesService.getCoursesDto({ InstructorId: user.id, PageSize: 200 })
        : this.coursesService.getCoursesDto({ IsPublished: true, PageSize: 200 });

    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: courses => {
        const rooms = user.role === 2 ? this.mapInstructorRooms(courses) : this.mapStudentRooms(courses, user.id);
        this.rooms.set(rooms);
        this.loadingRooms.set(false);

        if (!rooms.length) {
          return;
        }

        const nextRoom = rooms[0];
        this.selectedClassGroupId.set(nextRoom.classGroupId);
        this.selectedParticipantId.set(null);
        this.loadMessages(nextRoom.classGroupId, null);
        void this.connectRealtime(nextRoom.classGroupId);
      },
      error: () => {
        this.loadingRooms.set(false);
        this.toastr.error('Nao foi possivel carregar as turmas do chat.');
      }
    });
  }

  private loadMessages(classGroupId: string, participantId: string | null): void {
    this.loadingMessages.set(true);
    this.messages.set([]);

    this.chatMessagesService
      .getMessages(classGroupId, participantId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: messages => {
          this.messages.set(messages);
          this.loadingMessages.set(false);
          this.scheduleScrollToBottom();
        },
        error: () => {
          this.loadingMessages.set(false);
          this.toastr.error('Nao foi possivel carregar o historico do chat.');
        }
      });
  }

  private async connectRealtime(classGroupId: string): Promise<void> {
    try {
      await this.chatRealtimeService.connectToClassGroup(classGroupId, {
        onMessageReceived: message => this.upsertMessage(message),
        onMessageUpdated: message => this.upsertMessage(message),
        onMessageDeleted: messageId => this.removeMessage(messageId),
        onPresenceSnapshot: users => this.onlineUsers.set(users)
      });
    } catch {
      this.toastr.warning('Conexao em tempo real do chat indisponivel no momento.');
    }
  }

  private upsertMessage(message: ChatMessageDto): void {
    if (!this.isMessageForCurrentConversation(message)) {
      return;
    }

    this.messages.update(current => {
      const next = [...current];
      const existingIndex = next.findIndex(item => item.Id === message.Id);
      if (existingIndex >= 0) {
        next[existingIndex] = message;
      } else {
        next.push(message);
      }
      return next;
    });

    this.scheduleScrollToBottom();
  }

  private removeMessage(messageId: string): void {
    this.messages.update(current => current.filter(message => message.Id !== messageId));
  }

  private isMessageForCurrentConversation(message: ChatMessageDto): boolean {
    if (message.ClassGroupId !== this.selectedClassGroupId()) {
      return false;
    }

    const participantId = this.selectedParticipantId();
    const currentUserId = this.currentUser()?.id;
    if (!participantId) {
      return !message.RecipientId;
    }

    if (!currentUserId) {
      return false;
    }

    return (
      (message.SenderId === currentUserId && message.RecipientId === participantId) ||
      (message.SenderId === participantId && message.RecipientId === currentUserId)
    );
  }

  private mapInstructorRooms(courses: CourseDto[]): ChatRoom[] {
    return courses.flatMap(course =>
      (course.ClassGroups ?? [])
        .filter(group => group.EnableChat)
        .map(group => ({
          classGroupId: group.Id,
          classGroupName: group.Name,
          courseTitle: course.Title,
          participants: this.mapParticipants(course, group.Enrollments ?? [], this.currentUser()?.id ?? '')
        }))
    );
  }

  private mapStudentRooms(courses: CourseDto[], studentId: string): ChatRoom[] {
    return courses.flatMap(course =>
      (course.ClassGroups ?? [])
        .filter(group => group.EnableChat)
        .filter(group => (group.Enrollments ?? []).some(enrollment => enrollment.StudentId === studentId && enrollment.Status === 2))
        .map(group => ({
          classGroupId: group.Id,
          classGroupName: group.Name,
          courseTitle: course.Title,
          participants: this.mapParticipants(course, group.Enrollments ?? [], studentId)
        }))
    );
  }

  private mapParticipants(course: CourseDto, enrollments: ClassEnrollmentDto[], currentUserId: string): ChatParticipant[] {
    const participants = new Map<string, ChatParticipant>();

    if (course.InstructorId && course.InstructorId !== currentUserId) {
      participants.set(course.InstructorId, {
        userId: course.InstructorId,
        userName: course.InstructorName || 'Professor',
        roleLabel: 'Professor'
      });
    }

    enrollments
      .filter(enrollment => enrollment.Status === 2)
      .forEach(enrollment => {
        if (enrollment.StudentId === currentUserId || participants.has(enrollment.StudentId)) {
          return;
        }

        participants.set(enrollment.StudentId, {
          userId: enrollment.StudentId,
          userName: enrollment.StudentName || 'Aluno',
          roleLabel: 'Aluno'
        });
      });

    return [...participants.values()].sort((a, b) => a.userName.localeCompare(b.userName, 'pt-BR'));
  }

  private sortMessages(messages: ChatMessageDto[]): ChatMessageDto[] {
    return [...messages].sort((a, b) => {
      const sentAtDiff = this.parseSentAt(a.SentAt) - this.parseSentAt(b.SentAt);
      if (sentAtDiff !== 0) {
        return sentAtDiff;
      }

      const sentAtTextDiff = (a.SentAt ?? '').localeCompare(b.SentAt ?? '', 'pt-BR');
      if (sentAtTextDiff !== 0) {
        return sentAtTextDiff;
      }

      return a.Id.localeCompare(b.Id, 'pt-BR');
    });
  }

  private scheduleScrollToBottom(): void {
    setTimeout(() => {
      const anchor = this.messagesBottomAnchor()?.nativeElement;
      if (anchor) {
        anchor.scrollIntoView({ block: 'end' });
        return;
      }

      const container = this.messagesContainer()?.nativeElement;
      if (!container) {
        return;
      }

      container.scrollTop = container.scrollHeight;
    });
  }

  private parseSentAt(value: string | null | undefined): number {
    if (!value) {
      return Number.MIN_SAFE_INTEGER;
    }

    const parsed = Date.parse(value);
    if (!Number.isNaN(parsed)) {
      return parsed;
    }

    const normalized = value.includes('Z') || /[+-]\d{2}:\d{2}$/.test(value)
      ? value
      : `${value}Z`;
    const normalizedParsed = Date.parse(normalized);
    return Number.isNaN(normalizedParsed) ? Number.MIN_SAFE_INTEGER : normalizedParsed;
  }
}
