import { CommonModule, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnChanges, OnDestroy, SecurityContext, SimpleChanges, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DomSanitizer } from '@angular/platform-browser';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import {
  AttachmentDto,
  CommentDto,
  CommentHistoryDto,
  CommentReactionDto,
  MentionUserDto
} from '../api/discussion-data-contracts';
import { DiscussionApi } from "../api/discussion.api";

type HistoryState = {
  loading: boolean;
  opened: boolean;
  items: DiscussionHistoryItem[];
};

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
  providers: [DiscussionApi],
  imports: [CommonModule, FormsModule, AvatarModule, ButtonModule, InputTextModule, Textarea, DatePipe],
  template: `
    <section class="discussion-shell">
      <header class="discussion-header">
        <div>
          <h3>Discussion</h3>
          <p>Comments for entity {{ objectTypeId }} / {{ objectId }}</p>
        </div>
        <p-button icon="pi pi-refresh" [text]="true" (onClick)="loadComments()" [disabled]="loading" />
      </header>

      <div class="composer">
        <div class="composer-toolbar">
          <div class="composer-actions">
            <button type="button" class="toolbar-button" (click)="previewMode = !previewMode">
              {{ previewMode ? 'Edit' : 'Preview' }}
            </button>
            <label class="toolbar-button file-button">
              Attach files
              <input type="file" multiple (change)="onFilesSelected($event)" />
            </label>
          </div>
          <span class="composer-hint">Markdown supported. Mentions: @email or @first.last</span>
        </div>

        @if (!previewMode) {
          <textarea
            pTextarea
            class="composer-textarea"
            [autoResize]="true"
            rows="4"
            [(ngModel)]="newComment"
            (ngModelChange)="onComposerInput()"
            placeholder="Write a comment"
          ></textarea>
        } @else {
          <div class="markdown-preview" [innerHTML]="renderMarkdown(newComment)"></div>
        }

        @if (mentionSuggestions.length > 0) {
          <div class="mention-suggestions">
            @for (candidate of mentionSuggestions; track candidate.userId) {
              <button type="button" class="mention-item" (click)="insertMention(candidate)">
                <span>{{ candidate.displayName }}</span>
                <small>{{ candidate.email }}</small>
              </button>
            }
          </div>
        }

        @if (pendingFiles.length > 0) {
          <div class="pending-files">
            @for (file of pendingFiles; track file.name + file.size) {
              <div class="pending-file">
                <span>{{ file.name }}</span>
                <button type="button" (click)="removePendingFile(file)">Remove</button>
              </div>
            }
          </div>
        }

        <div class="composer-footer">
          <span class="error-text" *ngIf="errorMessage">{{ errorMessage }}</span>
          <p-button
            label="Send"
            icon="pi pi-send"
            (onClick)="createComment()"
            [disabled]="submitting || !newComment.trim()"
          />
        </div>
      </div>

      @if (loading) {
        <div class="state-card">Loading comments...</div>
      } @else if (comments.length === 0) {
        <div class="state-card">No comments yet.</div>
      } @else {
        <div class="comments-list">
          @for (comment of comments; track comment.commentId) {
            <article class="comment-card">
              <div class="comment-side">
                <p-avatar
                  [label]="comment.authorDisplayName.slice(0, 2).toUpperCase()"
                  shape="circle"
                  styleClass="comment-avatar"
                />
              </div>

              <div class="comment-body">
                <div class="comment-meta">
                  <div>
                    <strong>{{ comment.authorDisplayName }}</strong>
                    <span class="comment-date">{{ comment.createdAt | date: 'short' }}</span>
                    @if (comment.isEdited) {
                      <span class="comment-edited">edited</span>
                    }
                  </div>
                  <div class="comment-actions">
                    @if (comment.canEdit) {
                      <button type="button" class="link-button" (click)="startEdit(comment)">Edit</button>
                    }
                    @if (comment.canDelete) {
                      <button type="button" class="link-button danger" (click)="deleteComment(comment)">Delete</button>
                    }
                    @if (comment.historyCount > 0) {
                      <button type="button" class="link-button" (click)="toggleHistory(comment)">
                        History ({{ comment.historyCount }})
                      </button>
                    }
                  </div>
                </div>

                @if (editingCommentId === comment.commentId) {
                  <div class="editor-block">
                    <textarea
                      pTextarea
                      class="composer-textarea"
                      [autoResize]="true"
                      rows="4"
                      [(ngModel)]="editContent"
                      placeholder="Edit comment"
                    ></textarea>
                    <div class="editor-actions">
                      <p-button label="Save" (onClick)="saveEdit(comment)" [disabled]="!editContent.trim()" />
                      <p-button label="Cancel" [text]="true" (onClick)="cancelEdit()" />
                    </div>
                  </div>
                } @else {
                  <div class="markdown-preview" [innerHTML]="renderMarkdown(comment.content)"></div>
                }

                @if (comment.attachments.length > 0) {
                  <div class="attachment-list">
                    @for (attachment of comment.attachments; track attachment.attachmentId) {
                      <div class="attachment-item">
                        <button type="button" class="attachment-link" (click)="downloadAttachment(attachment)">
                          {{ attachment.fileName }}
                        </button>
                        <small>{{ formatBytes(attachment.fileSize) }}</small>
                        @if (comment.canEdit) {
                          <button type="button" class="link-button danger" (click)="deleteAttachment(comment, attachment)">
                            Remove
                          </button>
                        }
                      </div>
                    }
                  </div>
                }

                @if (comment.canEdit) {
                  <div class="inline-upload">
                    <label class="toolbar-button file-button">
                      Add attachment
                      <input type="file" multiple (change)="onInlineFilesSelected(comment, $event)" />
                    </label>
                  </div>
                }

                <div class="reaction-row">
                  @for (reaction of comment.reactions; track reaction.emojiCode) {
                    <button
                      type="button"
                      class="reaction-chip"
                      [class.active]="reaction.reactedByCurrentUser"
                      (click)="toggleReaction(comment, reaction)"
                    >
                      {{ reaction.emojiCode }} {{ reaction.count }}
                    </button>
                  }
                  @for (emoji of emojiPalette; track emoji) {
                    <button
                      type="button"
                      class="reaction-chip muted"
                      (click)="reactWithEmoji(comment, emoji)"
                    >
                      {{ emoji }}
                    </button>
                  }
                </div>

                @if (historyByComment[comment.commentId]?.opened) {
                  <div class="history-block">
                    @if (historyByComment[comment.commentId]?.loading) {
                      <div>Loading history...</div>
                    } @else {
                      @for (item of historyByComment[comment.commentId]?.items ?? []; track item.commentEditHistoryId) {
                        <div class="history-item">
                          <div class="history-title">
                            {{ item.editedByDisplayName }} · {{ item.editedAt | date: 'short' }}
                          </div>
                          <div class="history-columns">
                            <div>
                              <small>Before</small>
                              <div class="history-markdown" [innerHTML]="renderMarkdown(item.oldContent)"></div>
                            </div>
                            <div>
                              <small>After</small>
                              <div class="history-markdown" [innerHTML]="renderMarkdown(item.newContent)"></div>
                            </div>
                          </div>
                        </div>
                      }
                    }
                  </div>
                }
              </div>
            </article>
          }
        </div>
      }
    </section>
  `,
  styles: [
    `
      :host {
        display: block;
      }

      .discussion-shell {
        display: flex;
        flex-direction: column;
        gap: 1rem;
      }

      .discussion-header,
      .composer-toolbar,
      .composer-footer,
      .comment-meta,
      .comment-actions,
      .editor-actions,
      .reaction-row,
      .attachment-item,
      .pending-file {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 0.75rem;
      }

      .discussion-header h3,
      .discussion-header p {
        margin: 0;
      }

      .discussion-header p,
      .composer-hint,
      .comment-date,
      .comment-edited,
      .attachment-item small {
        color: var(--text-color-secondary, #64748b);
      }

      .composer,
      .comment-card,
      .state-card {
        border: 1px solid var(--surface-border, #dbe4f0);
        border-radius: 1rem;
        background: linear-gradient(180deg, rgba(248, 250, 252, 0.95), rgba(255, 255, 255, 0.98));
        padding: 1rem;
      }

      .composer-textarea {
        width: 100%;
      }

      .markdown-preview,
      .history-markdown {
        line-height: 1.6;
        word-break: break-word;
      }

      .comments-list {
        display: flex;
        flex-direction: column;
        gap: 1rem;
      }

      .comment-card {
        display: grid;
        grid-template-columns: 3rem 1fr;
        gap: 1rem;
      }

      .comment-avatar {
        background: #dbeafe;
        color: #1d4ed8;
      }

      .comment-body,
      .editor-block,
      .history-block,
      .attachment-list,
      .pending-files {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
      }

      .comment-actions,
      .composer-actions {
        flex-wrap: wrap;
      }

      .link-button,
      .toolbar-button,
      .attachment-link,
      .mention-item {
        border: none;
        background: transparent;
        padding: 0;
        cursor: pointer;
        text-align: left;
        color: #0f172a;
      }

      .danger {
        color: #b91c1c;
      }

      .file-button {
        position: relative;
        overflow: hidden;
      }

      .file-button input {
        position: absolute;
        inset: 0;
        opacity: 0;
        cursor: pointer;
      }

      .mention-suggestions {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
        border: 1px solid var(--surface-border, #dbe4f0);
        border-radius: 0.75rem;
        padding: 0.5rem;
        background: #fff;
      }

      .mention-item {
        display: flex;
        justify-content: space-between;
        gap: 1rem;
        padding: 0.35rem 0.5rem;
        border-radius: 0.5rem;
      }

      .mention-item:hover {
        background: #eff6ff;
      }

      .attachment-item {
        justify-content: flex-start;
        flex-wrap: wrap;
      }

      .attachment-link {
        color: #1d4ed8;
      }

      .reaction-row {
        justify-content: flex-start;
        flex-wrap: wrap;
      }

      .reaction-chip {
        border: 1px solid #cbd5e1;
        border-radius: 999px;
        background: #fff;
        padding: 0.25rem 0.65rem;
        cursor: pointer;
      }

      .reaction-chip.active {
        border-color: #1d4ed8;
        background: #dbeafe;
      }

      .reaction-chip.muted {
        opacity: 0.65;
      }

      .history-block {
        border-top: 1px dashed #cbd5e1;
        padding-top: 0.75rem;
      }

      .history-item {
        border: 1px solid #e2e8f0;
        border-radius: 0.75rem;
        padding: 0.75rem;
      }

      .history-title {
        font-weight: 600;
        margin-bottom: 0.5rem;
      }

      .history-columns {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 0.75rem;
      }

      .error-text {
        color: #b91c1c;
      }

      @media (max-width: 720px) {
        .comment-card {
          grid-template-columns: 1fr;
        }

        .history-columns {
          grid-template-columns: 1fr;
        }
      }
    `
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DiscussionComponent implements OnChanges, OnDestroy {
  private readonly discussionApi = inject(DiscussionApi);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly cdr = inject(ChangeDetectorRef);

  @Input({ required: true }) objectTypeId: number = 1;
  @Input({ required: true }) objectId: number = 1;

  comments: DiscussionComment[] = [];
  loading = false;
  submitting = false;
  previewMode = false;
  newComment = '';
  editContent = '';
  editingCommentId: number | null = null;
  errorMessage = '';
  pendingFiles: File[] = [];
  mentionSuggestions: DiscussionMentionUser[] = [];
  historyByComment: Record<number, HistoryState> = {};
  emojiPalette = ['👍', '👎', '🐳', '🍆', '🍑' ];
  private mentionSearchTimer: ReturnType<typeof setTimeout> | null = null;

  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['objectTypeId'] || changes['objectId']) && this.objectTypeId && this.objectId) {
      this.loadComments();
    }
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
    this.errorMessage = '';
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
      this.errorMessage = this.extractErrorMessage(error, 'Failed to load comments.');
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
    this.errorMessage = '';
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
      this.errorMessage = this.extractErrorMessage(error, 'Failed to create comment.');
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
      const updated = await this.discussionApi.update(
        { id: comment.commentId },
        {
        content: this.editContent.trim()
        }
      );
      this.comments = this.comments.map((item) => (item.commentId === comment.commentId ? this.normalizeComment(updated) : item));
      this.cancelEdit();
    } catch (error) {
      this.errorMessage = this.extractErrorMessage(error, 'Failed to update comment.');
    } finally {
      this.cdr.markForCheck();
    }
  }

  async deleteComment(comment: DiscussionComment): Promise<void> {
    try {
      await this.discussionApi.delete({ id: comment.commentId });
      this.comments = this.comments.filter((item) => item.commentId !== comment.commentId);
    } catch (error) {
      this.errorMessage = this.extractErrorMessage(error, 'Failed to delete comment.');
    } finally {
      this.cdr.markForCheck();
    }
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
      this.errorMessage = this.extractErrorMessage(error, 'Failed to load edit history.');
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
      this.errorMessage = this.extractErrorMessage(error, 'Failed to upload attachment.');
      this.cdr.markForCheck();
    }
  }

  async deleteAttachment(comment: DiscussionComment, attachment: DiscussionAttachment): Promise<void> {
    try {
      await this.discussionApi.deleteAttachment({ id: attachment.attachmentId });
      this.comments = this.comments.map((item) =>
        item.commentId === comment.commentId
          ? { ...item, attachments: item.attachments.filter((entry) => entry.attachmentId !== attachment.attachmentId) }
          : item
      );
    } catch (error) {
      this.errorMessage = this.extractErrorMessage(error, 'Failed to delete attachment.');
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
      this.errorMessage = this.extractErrorMessage(error, 'Failed to download attachment.');
      this.cdr.markForCheck();
    }
  }

  async toggleReaction(comment: DiscussionComment, reaction: DiscussionReaction): Promise<void> {
    try {
      const reactions = reaction.reactedByCurrentUser
        ? await this.discussionApi.removeReaction({ commentId: comment.commentId, emojiCode: reaction.emojiCode })
        : await this.discussionApi.addReaction({ commentId: comment.commentId, emojiCode: reaction.emojiCode });
      this.applyReactions(comment.commentId, reactions.map((item) => this.normalizeReaction(item)));
    } catch (error) {
      this.errorMessage = this.extractErrorMessage(error, 'Failed to update reaction.');
    } finally {
      this.cdr.markForCheck();
    }
  }

  async reactWithEmoji(comment: DiscussionComment, emojiCode: string): Promise<void> {
    try {
      const reactions = await this.discussionApi.addReaction({ commentId: comment.commentId, emojiCode });
      this.applyReactions(comment.commentId, reactions.map((item) => this.normalizeReaction(item)));
    } catch (error) {
      this.errorMessage = this.extractErrorMessage(error, 'Failed to add reaction.');
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
    const escaped = markdown
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;');

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
      return `${value} B`;
    }
    if (value < 1024 * 1024) {
      return `${(value / 1024).toFixed(1)} KB`;
    }
    return `${(value / (1024 * 1024)).toFixed(1)} MB`;
  }

  private applyReactions(commentId: number, reactions: DiscussionReaction[]): void {
    this.comments = this.comments.map((item) =>
      item.commentId === commentId ? { ...item, reactions } : item
    );
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

  private extractErrorMessage(error: unknown, fallback: string): string {
    if (typeof error === 'object' && error && 'message' in error && typeof error.message === 'string') {
      return error.message;
    }

    return fallback;
  }
}
