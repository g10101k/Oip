import { CommonModule, DatePipe, formatDate } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  Input,
  LOCALE_ID,
  OnChanges,
  OnDestroy,
  SecurityContext,
  SimpleChanges,
  inject,
  OnInit
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DomSanitizer } from '@angular/platform-browser';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { PopoverModule } from 'primeng/popover';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { SelectButton } from 'primeng/selectbutton';
import { Textarea } from 'primeng/textarea';
import { TooltipModule } from 'primeng/tooltip';
import {
  AttachmentDto,
  CommentDto,
  CommentHistoryDto,
  CommentReactionDto,
  MentionUserDto
} from '../../api/discussion-data-contracts';
import { DiscussionApi } from '../../api/discussion.api';
import { MsgService } from '../../services/msg.service';
import { LayoutService } from '../../services/app.layout.service';
import { getInitialsFromString } from '../../helpers/avatar.helper';
import { provideTranslations } from '../../helpers/l10n.helper';

import en from './l10n/discussion.en.json';
import ru from './l10n/discussion.ru.json';

type HistoryState = {
  loading: boolean;
  opened: boolean;
  items: DiscussionHistoryItem[];
};

type SortOrder = 'newest' | 'oldest';

type DiscussionAttachment = Required<AttachmentDto>;
type DiscussionReaction = Required<CommentReactionDto>;
type DiscussionMentionUser = Required<MentionUserDto>;
type DiscussionComment = Omit<CommentDto, 'attachments' | 'reactions' | 'mentions'> & {
  commentId: number;
  content: string;
  authorDisplayName: string;
  authorEmail: string;
  isEdited: boolean;
  historyCount: number;
  canEdit: boolean;
  canDelete: boolean;
  attachments: DiscussionAttachment[];
  reactions: DiscussionReaction[];
};
type DiscussionHistoryItem = Required<CommentHistoryDto>;

@Component({
  selector: 'discussion',
  standalone: true,
  providers: [ConfirmationService, DiscussionApi],
  imports: [
    CommonModule,
    FormsModule,
    AvatarModule,
    ButtonModule,
    ConfirmDialog,
    PopoverModule,
    ProgressSpinnerModule,
    SelectButton,
    Textarea,
    TooltipModule,
    DatePipe,
    TranslatePipe
  ],
  templateUrl: './discussion.component.html',
  styleUrl: './discussion.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DiscussionComponent implements OnChanges, OnDestroy, OnInit {
  private readonly msgService = inject(MsgService);
  private readonly translations = provideTranslations({ en, ru });

  private readonly discussionApi = inject(DiscussionApi);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly translateService = inject(TranslateService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly confirmationService = inject(ConfirmationService);
  protected readonly layoutService = inject(LayoutService);
  private readonly locale = inject(LOCALE_ID);

  @Input({ required: true }) objectTypeId = 1;
  @Input({ required: true }) objectId = 1;

  comments: DiscussionComment[] = [];
  loading = false;
  submitting = false;
  previewMode = false;
  newComment = '';
  editContent = '';
  editingCommentId: number | null = null;
  pendingFiles: File[] = [];
  mentionSuggestions: DiscussionMentionUser[] = [];
  historyByComment: Record<number, HistoryState> = {};
  emojiPalette = ['👍', '👎', '🐳'];
  sortOrder: SortOrder = 'newest';
  readonly sortOptions = [
    { label: this.translateService.instant('discussionComponent.newestFirst'), value: 'newest' },
    { label: this.translateService.instant('discussionComponent.oldestFirst'), value: 'oldest' }
  ];

  /** Comments in the order the user picked. */
  get orderedComments(): DiscussionComment[] {
    const direction = this.sortOrder === 'newest' ? -1 : 1;
    return [...this.comments].sort(
      (a, b) => direction * (new Date(a.createdAt ?? 0).getTime() - new Date(b.createdAt ?? 0).getTime())
    );
  }
  private mentionSearchTimer: ReturnType<typeof setTimeout> | null = null;

  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['objectTypeId'] || changes['objectId']) && this.objectTypeId && this.objectId) {
      this.loadComments().then();
    }
  }

  async ngOnInit() {
    await this.loadComments();
  }

  ngOnDestroy(): void {
    if (this.mentionSearchTimer) {
      clearTimeout(this.mentionSearchTimer);
    }
  }

  async loadComments(): Promise<void> {
    if (!this.objectTypeId || !this.objectId) {
      return;
    }

    this.loading = true;
    this.cdr.markForCheck();

    try {
      const items = await this.discussionApi.getByObject({
        objectTypeId: this.objectTypeId,
        objectId: this.objectId,
        skip: 0,
        take: 50
      });
      this.comments = items.map((item) => this.normalizeComment(item));
    } catch (error) {
      this.msgService.errorFromException(
        error,
        this.translateService.instant('discussionComponent.errors.loadComments')
      );
    } finally {
      this.loading = false;
      this.cdr.markForCheck();
    }
  }

  async createComment(): Promise<void> {
    const content = this.newComment.trim();
    if (!content) {
      return;
    }

    this.submitting = true;
    this.cdr.markForCheck();

    try {
      const created = await this.discussionApi.create({
        objectTypeId: this.objectTypeId,
        objectId: this.objectId,
        content
      });

      if (this.pendingFiles.length > 0) {
        for (const file of this.pendingFiles) {
          await this.discussionApi.uploadAttachment({
            CommentId: created.commentId as number,
            File: file
          });
        }
      }

      this.newComment = '';
      this.pendingFiles = [];
      this.previewMode = false;
      this.mentionSuggestions = [];
      await this.loadComments();
    } catch (error) {
      this.msgService.errorFromException(
        error,
        this.translateService.instant('discussionComponent.errors.createComment')
      );
    } finally {
      this.submitting = false;
      this.cdr.markForCheck();
    }
  }

  startEdit(comment: DiscussionComment): void {
    this.editingCommentId = comment.commentId;
    this.editContent = comment.content;
  }

  cancelEdit(): void {
    this.editingCommentId = null;
    this.editContent = '';
  }

  async saveEdit(comment: DiscussionComment): Promise<void> {
    try {
      const updated = await this.discussionApi.update({ id: comment.commentId }, { content: this.editContent.trim() });
      this.comments = this.comments.map((item) =>
        item.commentId === comment.commentId ? this.normalizeComment(updated) : item
      );
      this.cancelEdit();
    } catch (error) {
      this.msgService.errorFromException(
        error,
        this.translateService.instant('discussionComponent.errors.updateComment')
      );
    } finally {
      this.cdr.markForCheck();
    }
  }

  deleteComment(comment: DiscussionComment): void {
    this.confirmationService.confirm({
      header: this.translateService.instant('discussionComponent.warning'),
      message: this.translateService.instant('discussionComponent.confirmDeleteComment'),
      icon: 'pi pi-trash',
      rejectButtonProps: {
        label: this.translateService.instant('discussionComponent.cancel'),
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: this.translateService.instant('discussionComponent.delete'),
        severity: 'danger'
      },
      accept: async () => {
        await this.performDeleteComment(comment);
      }
    });
  }

  async toggleHistory(comment: DiscussionComment): Promise<void> {
    const current = this.historyByComment[comment.commentId];
    if (current?.opened) {
      this.historyByComment[comment.commentId] = { ...current, opened: false };
      return;
    }

    this.historyByComment[comment.commentId] = {
      opened: true,
      loading: true,
      items: current?.items ?? []
    };
    this.cdr.markForCheck();

    try {
      const items = await this.discussionApi.getHistory({ id: comment.commentId });
      this.historyByComment[comment.commentId] = {
        opened: true,
        loading: false,
        items: items.map((item) => this.normalizeHistoryItem(item))
      };
    } catch (error) {
      this.msgService.errorFromException(
        error,
        this.translateService.instant('discussionComponent.errors.loadEditHistory')
      );
      this.historyByComment[comment.commentId] = {
        opened: false,
        loading: false,
        items: []
      };
    } finally {
      this.cdr.markForCheck();
    }
  }

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.pendingFiles = [...this.pendingFiles, ...Array.from(input.files ?? [])];
    input.value = '';
  }

  removePendingFile(file: File): void {
    this.pendingFiles = this.pendingFiles.filter((item) => item !== file);
  }

  async onInlineFilesSelected(comment: DiscussionComment, event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    input.value = '';

    try {
      for (const file of files) {
        await this.discussionApi.uploadAttachment({
          CommentId: comment.commentId,
          File: file
        });
      }
      await this.loadComments();
    } catch (error) {
      this.msgService.errorFromException(
        error,
        this.translateService.instant('discussionComponent.errors.uploadAttachment')
      );
      this.cdr.markForCheck();
    }
  }

  deleteAttachment(comment: DiscussionComment, attachment: DiscussionAttachment): void {
    this.confirmationService.confirm({
      header: this.translateService.instant('discussionComponent.warning'),
      message: this.translateService.instant('discussionComponent.confirmDeleteFile', {
        fileName: attachment.fileName
      }),
      icon: 'pi pi-trash',
      rejectButtonProps: {
        label: this.translateService.instant('discussionComponent.cancel'),
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: this.translateService.instant('discussionComponent.delete'),
        severity: 'danger'
      },
      accept: async () => {
        await this.performDeleteAttachment(comment, attachment);
      }
    });
  }

  private async performDeleteComment(comment: DiscussionComment): Promise<void> {
    try {
      await this.discussionApi.delete({ id: comment.commentId });
      this.comments = this.comments.filter((item) => item.commentId !== comment.commentId);
    } catch (error) {
      this.msgService.errorFromException(
        error,
        this.translateService.instant('discussionComponent.errors.deleteComment')
      );
    } finally {
      this.cdr.markForCheck();
    }
  }

  private async performDeleteAttachment(comment: DiscussionComment, attachment: DiscussionAttachment): Promise<void> {
    try {
      await this.discussionApi.deleteAttachment({ id: attachment.attachmentId });
      this.comments = this.comments.map((item) =>
        item.commentId === comment.commentId
          ? { ...item, attachments: item.attachments.filter((entry) => entry.attachmentId !== attachment.attachmentId) }
          : item
      );
    } catch (error) {
      this.msgService.errorFromException(
        error,
        this.translateService.instant('discussionComponent.errors.deleteAttachment')
      );
    } finally {
      this.cdr.markForCheck();
    }
  }

  async downloadAttachment(attachment: DiscussionAttachment): Promise<void> {
    try {
      const blob = (await this.discussionApi.getAttachmentContent(
        { id: attachment.attachmentId },
        { format: 'blob' }
      )) as unknown as Blob;
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = attachment.fileName;
      link.click();
      URL.revokeObjectURL(url);
    } catch (error) {
      this.msgService.errorFromException(
        error,
        this.translateService.instant('discussionComponent.errors.downloadAttachment')
      );
      this.cdr.markForCheck();
    }
  }

  async toggleReaction(comment: DiscussionComment, reaction: DiscussionReaction): Promise<void> {
    try {
      const reactions = reaction.reactedByCurrentUser
        ? await this.discussionApi.removeReaction({ commentId: comment.commentId, emojiCode: reaction.emojiCode })
        : await this.discussionApi.addReaction({ commentId: comment.commentId, emojiCode: reaction.emojiCode });
      this.applyReactions(
        comment.commentId,
        reactions.map((item) => this.normalizeReaction(item))
      );
    } catch (error) {
      this.msgService.errorFromException(
        error,
        this.translateService.instant('discussionComponent.errors.updateReaction')
      );
    } finally {
      this.cdr.markForCheck();
    }
  }

  async reactWithEmoji(comment: DiscussionComment, emojiCode: string, popover?: { hide: () => void }): Promise<void> {
    try {
      const reactions = await this.discussionApi.addReaction({ commentId: comment.commentId, emojiCode });
      this.applyReactions(
        comment.commentId,
        reactions.map((item) => this.normalizeReaction(item))
      );
      popover?.hide();
    } catch (error) {
      this.msgService.errorFromException(
        error,
        this.translateService.instant('discussionComponent.errors.addReaction')
      );
    } finally {
      this.cdr.markForCheck();
    }
  }

  onComposerInput(): void {
    if (this.mentionSearchTimer) {
      clearTimeout(this.mentionSearchTimer);
    }

    const token = this.extractMentionQuery(this.newComment);
    if (!token || token.length < 2) {
      this.mentionSuggestions = [];
      this.cdr.markForCheck();
      return;
    }

    this.mentionSearchTimer = setTimeout(async () => {
      try {
        const users = await this.discussionApi.searchMentionUsers({ term: token });
        this.mentionSuggestions = users.map((item) => this.normalizeMentionUser(item));
      } catch {
        this.mentionSuggestions = [];
      } finally {
        this.cdr.markForCheck();
      }
    }, 250);
  }

  insertMention(candidate: DiscussionMentionUser): void {
    const query = this.extractMentionQuery(this.newComment);
    if (!query) {
      return;
    }

    this.newComment = this.newComment.replace(/@[\w.+\-@]*$/, `@${candidate.email} `);
    this.mentionSuggestions = [];
  }

  renderMarkdown(markdown: string): string {
    const escaped = markdown.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

    const html = escaped
      .replace(/^### (.*)$/gm, '<h3>$1</h3>')
      .replace(/^## (.*)$/gm, '<h2>$1</h2>')
      .replace(/^# (.*)$/gm, '<h1>$1</h1>')
      .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
      .replace(/\*(.+?)\*/g, '<em>$1</em>')
      .replace(/`(.+?)`/g, '<code>$1</code>')
      .replace(/\[([^\]]+)\]\((https?:\/\/[^\s]+)\)/g, '<a href="$2" target="_blank" rel="noopener">$1</a>')
      .replace(/(^|\s)@([A-Za-z0-9._+\-@]+)/g, '$1<span class="mention-token">@$2</span>')
      .replace(/\n/g, '<br>');

    return this.sanitizer.sanitize(SecurityContext.HTML, html) ?? '';
  }

  formatBytes(value: number): string {
    if (value < 1024) {
      return `${value} ${this.translateService.instant('discussionComponent.units.bytes')}`;
    }
    if (value < 1024 * 1024) {
      return `${(value / 1024).toFixed(1)} ${this.translateService.instant('discussionComponent.units.kilobytes')}`;
    }
    return `${(value / (1024 * 1024)).toFixed(1)} ${this.translateService.instant('discussionComponent.units.megabytes')}`;
  }

  /** Describes how long ago the date was, e.g. "2 hours ago"; dates older than a week are shown in full. */
  relativeTime(value: Date | string | null | undefined): string {
    if (!value) {
      return '';
    }

    const date = new Date(value);
    const seconds = Math.round((date.getTime() - Date.now()) / 1000);
    if (seconds < -7 * 86400) {
      return formatDate(date, this.layoutService.dateTimeFormat(), this.locale);
    }

    const units: [string, number][] = [
      ['days', 86400],
      ['hours', 3600],
      ['minutes', 60]
    ];
    for (const [unit, size] of units) {
      if (-seconds >= size) {
        return this.translateService.instant(`discussionComponent.ago.${unit}`, {
          count: Math.floor(-seconds / size)
        });
      }
    }

    return this.translateService.instant('discussionComponent.ago.now');
  }

  /** PrimeIcons class for a file of the given MIME type. */
  fileIcon(mimeType: string | null | undefined): string {
    const type = mimeType ?? '';
    if (type.startsWith('image/')) {
      return 'pi pi-image';
    }
    if (type === 'application/pdf') {
      return 'pi pi-file-pdf';
    }
    if (type.includes('sheet') || type.includes('excel') || type === 'text/csv') {
      return 'pi pi-file-excel';
    }
    if (type.includes('word')) {
      return 'pi pi-file-word';
    }
    return 'pi pi-file';
  }

  private applyReactions(commentId: number, reactions: DiscussionReaction[]): void {
    this.comments = this.comments.map((item) => (item.commentId === commentId ? { ...item, reactions } : item));
  }

  private normalizeComment(comment: CommentDto): DiscussionComment {
    return {
      ...comment,
      commentId: comment.commentId ?? 0,
      content: comment.content ?? '',
      authorDisplayName: comment.authorDisplayName ?? '',
      authorEmail: comment.authorEmail ?? '',
      isEdited: comment.isEdited ?? false,
      historyCount: comment.historyCount ?? 0,
      canEdit: comment.canEdit ?? false,
      canDelete: comment.canDelete ?? false,
      attachments: (comment.attachments ?? []).map((item) => this.normalizeAttachment(item)),
      reactions: (comment.reactions ?? []).map((item) => this.normalizeReaction(item))
    };
  }

  private normalizeAttachment(attachment: AttachmentDto): DiscussionAttachment {
    return {
      attachmentId: attachment.attachmentId ?? 0,
      fileName: attachment.fileName ?? '',
      fileType: attachment.fileType ?? '',
      fileSize: attachment.fileSize ?? 0,
      uploadedAt: attachment.uploadedAt ?? new Date(0),
      storageFileId: attachment.storageFileId ?? '',
      downloadUrl: attachment.downloadUrl ?? ''
    };
  }

  private normalizeReaction(reaction: CommentReactionDto): DiscussionReaction {
    return {
      emojiCode: reaction.emojiCode ?? '',
      count: reaction.count ?? 0,
      reactedByCurrentUser: reaction.reactedByCurrentUser ?? false
    };
  }

  private normalizeMentionUser(user: MentionUserDto): DiscussionMentionUser {
    return {
      userId: user.userId ?? 0,
      displayName: user.displayName ?? '',
      email: user.email ?? ''
    };
  }

  private normalizeHistoryItem(item: CommentHistoryDto): DiscussionHistoryItem {
    return {
      commentEditHistoryId: item.commentEditHistoryId ?? 0,
      oldContent: item.oldContent ?? '',
      newContent: item.newContent ?? '',
      editedByUserId: item.editedByUserId ?? 0,
      editedByDisplayName: item.editedByDisplayName ?? '',
      editedAt: item.editedAt ?? new Date(0)
    };
  }

  private extractMentionQuery(value: string): string | null {
    const match = value.match(/@([A-Za-z0-9._+\-@]*)$/);
    return match?.[1] ?? null;
  }

  protected readonly getInitialsFromString = getInitialsFromString;
}
