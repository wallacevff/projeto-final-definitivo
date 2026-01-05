import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, ElementRef, ViewChild, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { forkJoin } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

import { ForumService } from '../../core/services/forum.service';
import { ForumPostsService } from '../../core/services/forum-posts.service';
import { ForumPostDto, ForumThreadDto } from '../../core/api/forum.api';
import { AuthService } from '../../core/services/auth.service';
import { RichTextEditorComponent } from '../../shared/components/rich-text-editor/rich-text-editor.component';

interface ForumPostView {
  id: string;
  authorName: string;
  message: string;
  createdAt: string;
  replies: ForumPostView[];
  parentId?: string;
}

@Component({
  selector: 'app-forum-thread',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, RichTextEditorComponent],
  templateUrl: './forum-thread.component.html',
  styleUrl: './forum-thread.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ForumThreadComponent {
  @ViewChild('replySection') replySection?: ElementRef<HTMLElement>;
  private readonly forumService = inject(ForumService);
  private readonly postsService = inject(ForumPostsService);
  private readonly authService = inject(AuthService);
  private readonly toastr = inject(ToastrService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);
  private readonly sanitizer = inject(DomSanitizer);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly thread = signal<ForumThreadDto | null>(null);
  readonly posts = signal<ForumPostView[]>([]);
  readonly replyTarget = signal<ForumPostView | null>(null);
  readonly isSubmitting = signal(false);

  readonly form = this.fb.group({
    message: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(2000)])
  });

  readonly isLocked = computed(() => this.thread()?.IsLocked ?? false);
  readonly threadTitle = computed(() => this.thread()?.Title ?? 'Discussao');

  constructor() {
    this.route.paramMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        const threadId = params.get('threadId');
        if (!threadId) {
          this.error.set('Topico nao encontrado.');
          this.loading.set(false);
          return;
        }
        this.loadThread(threadId);
      });
  }

  trackByPost(_: number, post: ForumPostView): string {
    return post.id;
  }

  openReply(post: ForumPostView): void {
    if (this.isLocked()) {
      this.toastr.warning('Este topico esta bloqueado.');
      return;
    }
    this.replyTarget.set(post);
    this.scrollToReplyForm();
  }

  cancelReply(): void {
    this.replyTarget.set(null);
  }

  submit(): void {
    if (this.isLocked()) {
      this.toastr.warning('Este topico esta bloqueado.');
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const user = this.authService.currentUser();
    if (!user) {
      this.toastr.error('Usuario nao autenticado.');
      return;
    }

    const currentThread = this.thread();
    if (!currentThread) {
      return;
    }

    const message = this.form.controls.message.value.trim();
    if (!message) {
      this.form.controls.message.setErrors({ required: true });
      return;
    }

    this.isSubmitting.set(true);
    this.postsService
      .createPost({
        ThreadId: currentThread.Id,
        AuthorId: user.id,
        ParentPostId: this.replyTarget()?.id,
        Message: message
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastr.success('Mensagem enviada.');
          this.form.reset({ message: '' });
          this.replyTarget.set(null);
          this.loadPosts(currentThread.Id);
          this.isSubmitting.set(false);
        },
        error: () => {
          this.toastr.error('Nao foi possivel enviar sua mensagem.');
          this.isSubmitting.set(false);
        }
      });
  }

  safeHtml(content?: string | null): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(content ?? '');
  }

  private scrollToReplyForm(): void {
    const element = this.replySection?.nativeElement;
    if (!element) {
      return;
    }
    element.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  private loadThread(threadId: string): void {
    this.loading.set(true);
    forkJoin({
      thread: this.forumService.getThreadById(threadId),
      posts: this.postsService.getPosts({ ThreadId: threadId })
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ thread, posts }) => {
          this.thread.set(thread);
          this.posts.set(this.buildPostTree(posts));
          this.loading.set(false);
          this.error.set(null);
        },
        error: () => {
          this.error.set('Nao foi possivel carregar a discussao.');
          this.loading.set(false);
        }
      });
  }

  private loadPosts(threadId: string): void {
    this.postsService
      .getPosts({ ThreadId: threadId })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: posts => {
          this.posts.set(this.buildPostTree(posts));
        },
        error: () => {
          this.toastr.error('Nao foi possivel carregar as mensagens.');
        }
      });
  }

  private buildPostTree(posts: ForumPostDto[]): ForumPostView[] {
    const map = new Map<string, ForumPostView>();
    posts.forEach(post => {
      map.set(post.Id, {
        id: post.Id,
        authorName: post.AuthorName || 'Usuario',
        message: post.Message,
        createdAt: post.CreatedAt,
        replies: [],
        parentId: post.ParentPostId
      });
    });

    const roots: ForumPostView[] = [];
    map.forEach(post => {
      if (post.parentId && map.has(post.parentId)) {
        map.get(post.parentId)!.replies.push(post);
      } else {
        roots.push(post);
      }
    });

    const sortByDate = (items: ForumPostView[]) => {
      items.sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
      items.forEach(item => sortByDate(item.replies));
    };

    sortByDate(roots);
    return roots;
  }
}
