import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { ChatMessageDto, ChatPresenceUserDto } from '../../../core/api/chat.api';
import { CourseDto } from '../../../core/api/courses.api';
import { AuthService } from '../../../core/services/auth.service';
import { ChatMessagesService } from '../../../core/services/chat-messages.service';
import { ChatRealtimeService } from '../../../core/services/chat-realtime.service';
import { CoursesService } from '../../../core/services/courses.service';

interface ChatRoom {
  classGroupId: string;
  classGroupName: string;
  courseTitle: string;
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

  readonly currentUser = this.authService.currentUser;
  readonly isVisible = computed(() => !!this.currentUser());
  readonly isOpen = signal(false);
  readonly loadingRooms = signal(false);
  readonly loadingMessages = signal(false);
  readonly sendingMessage = signal(false);
  readonly rooms = signal<ChatRoom[]>([]);
  readonly selectedClassGroupId = signal<string | null>(null);
  readonly messages = signal<ChatMessageDto[]>([]);
  readonly onlineUsers = signal<ChatPresenceUserDto[]>([]);

  readonly selectedRoom = computed(() => {
    const currentId = this.selectedClassGroupId();
    return this.rooms().find(room => room.classGroupId === currentId) ?? null;
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

  selectRoom(room: ChatRoom): void {
    if (this.selectedClassGroupId() === room.classGroupId) {
      return;
    }

    this.selectedClassGroupId.set(room.classGroupId);
    this.loadMessages(room.classGroupId);
    void this.connectRealtime(room.classGroupId);
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
        this.loadMessages(nextRoom.classGroupId);
        void this.connectRealtime(nextRoom.classGroupId);
      },
      error: () => {
        this.loadingRooms.set(false);
        this.toastr.error('Nao foi possivel carregar as turmas do chat.');
      }
    });
  }

  private loadMessages(classGroupId: string): void {
    this.loadingMessages.set(true);
    this.messages.set([]);
    this.onlineUsers.set([]);

    this.chatMessagesService
      .getMessages(classGroupId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: messages => {
          this.messages.set(this.sortMessages(messages));
          this.loadingMessages.set(false);
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
    const selectedClassGroupId = this.selectedClassGroupId();
    if (selectedClassGroupId && message.ClassGroupId !== selectedClassGroupId) {
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
      return this.sortMessages(next);
    });
  }

  private removeMessage(messageId: string): void {
    this.messages.update(current => current.filter(message => message.Id !== messageId));
  }

  private mapInstructorRooms(courses: CourseDto[]): ChatRoom[] {
    return courses.flatMap(course =>
      (course.ClassGroups ?? [])
        .filter(group => group.EnableChat)
        .map(group => ({
          classGroupId: group.Id,
          classGroupName: group.Name,
          courseTitle: course.Title
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
          courseTitle: course.Title
        }))
    );
  }

  private sortMessages(messages: ChatMessageDto[]): ChatMessageDto[] {
    return [...messages].sort((a, b) => new Date(a.SentAt).getTime() - new Date(b.SentAt).getTime());
  }
}
