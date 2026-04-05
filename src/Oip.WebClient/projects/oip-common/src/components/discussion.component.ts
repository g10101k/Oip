import { CommonModule, DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  Input,
  OnChanges,
  OnDestroy,
  SecurityContext,
  SimpleChanges,
  inject, OnInit
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DomSanitizer } from '@angular/platform-browser';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { PanelModule } from 'primeng/panel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { Textarea } from 'primeng/textarea';
import {
  AttachmentDto,
  CommentDto,
  CommentHistoryDto,
  CommentReactionDto,
  MentionUserDto
} from '../api/discussion-data-contracts';
import { DiscussionApi } from '../api/discussion.api';

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
  imports: [
    CommonModule,
    FormsModule,
    AvatarModule,
    ButtonModule,
    CardModule,
    DividerModule,
    MessageModule,
    PanelModule,
    ProgressSpinnerModule,
    TagModule,
    Textarea,
    DatePipe
  ],
  template: `
    <section class="flex flex-col gap-4">
      <p-card class="block">
        <div class="flex flex-col gap-4">
          @if (!previewMode) {
            <textarea
              pTextarea
              class="min-h-28 w-full"
              [autoResize]="true"
              rows="5"
              [(ngModel)]="newComment"
              (ngModelChange)="onComposerInput()"
              placeholder="Write a comment. Use @email or @first.last for mentions."
            ></textarea>
          } @else {
            <div
              class="markdown-preview min-h-28 rounded-2xl border border-surface-200 bg-surface-50 px-4 py-3 leading-7 dark:border-surface-700 dark:bg-surface-900/40"
              [innerHTML]="renderMarkdown(newComment)"
            ></div>
          }

          @if (mentionSuggestions.length > 0) {
            <p-panel class="block">
              <ng-template pTemplate="header">
                <div class="inline-flex items-center gap-2 text-sm font-medium">
                  <i class="pi pi-at"></i>
                  <span>Mention suggestions</span>
                </div>
              </ng-template>

              <div class="flex flex-col gap-3">
                @for (candidate of mentionSuggestions; track candidate.userId) {
                  <button
                    type="button"
                    class="flex w-full items-center justify-between gap-4 rounded-xl border border-transparent px-3 py-3 text-left transition hover:border-primary-200 hover:bg-primary-50 dark:hover:border-primary-800 dark:hover:bg-primary-950/30"
                    (click)="insertMention(candidate)"
                  >
                    <div class="min-w-0">
                      <strong>{{ candidate.displayName }}</strong>
                      <small class="mt-1 block text-surface-500">{{ candidate.email }}</small>
                    </div>
                    <i class="pi pi-arrow-up-left shrink-0 text-surface-400"></i>
                  </button>
                }
              </div>
            </p-panel>
          }

          @if (pendingFiles.length > 0) {
            <div class="flex flex-col gap-3">
              @for (file of pendingFiles; track file.name + file.size) {
                <div
                  class="flex flex-col gap-3 rounded-2xl border border-surface-200 px-4 py-3 sm:flex-row sm:items-center sm:justify-between dark:border-surface-700">
                  <div class="inline-flex items-center gap-2">
                    <i class="pi pi-file"></i>
                    <span class="break-all">{{ file.name }}</span>
                    <p-tag [value]="formatBytes(file.size)" severity="contrast"/>
                  </div>
                  <p-button
                    icon="pi pi-times"
                    severity="danger"
                    [rounded]="true"
                    [text]="true"
                    (onClick)="removePendingFile(file)"
                  />
                </div>
              }
            </div>
          }

          @if (errorMessage) {
            <p-message severity="error" [textContent]="errorMessage"></p-message>
          }

          <div class="flex flex-col gap-2 sm:flex-row sm:justify-end">
            <p-button
              [label]="previewMode ? 'Edit' : 'Preview'"
              [icon]="previewMode ? 'pi pi-pencil' : 'pi pi-eye'"
              severity="secondary"
              [text]="true"
              (onClick)="previewMode = !previewMode"
            />

            <label class="file-trigger">
              <p-button
                label="Attach files"
                icon="pi pi-paperclip"
                severity="secondary"
                [text]="true"
              />
              <input type="file" multiple (change)="onFilesSelected($event)"/>
            </label>
            <p-button
              label="Send"
              icon="pi pi-send"
              [loading]="submitting"
              [disabled]="submitting || !newComment.trim()"
              (onClick)="createComment()"
            />
          </div>
        </div>
      </p-card>

      @if (loading) {
        <div
          class="flex min-h-20 flex-col items-center justify-center gap-3 rounded-2xl border border-surface-200 px-4 py-5 text-surface-500 dark:border-surface-700">
          <p-progressSpinner strokeWidth="4"/>
          <span>Loading comments...</span>
        </div>
      } @else if (comments.length === 0) {
        <div
          class="flex min-h-20 items-center justify-center gap-3 rounded-2xl border border-surface-200 px-4 py-5 text-surface-500 dark:border-surface-700">
          <i class="pi pi-inbox"></i>
          <span>No comments yet.</span>
        </div>
      } @else {
        <div class="flex flex-col gap-4">
          @for (comment of comments; track comment.commentId) {
            <p-card class="block">
              <ng-template pTemplate="content">
                <div class="grid gap-4 md:grid-cols-[3.2rem_minmax(0,1fr)]">
                  <div>
                    <p-avatar
                      [label]="comment.authorDisplayName.slice(0, 2).toUpperCase()"
                      shape="circle"
                    />
                  </div>

                  <div class="flex flex-col gap-2">
                    <div class="flex flex-row gap-3 justify-between">
                      <div class="flex flex-wrap items-center gap-2">
                        <strong>{{ comment.authorDisplayName }}</strong>
                        <span class="text-sm text-surface-500">{{ comment.createdAt | date: 'short' }}</span>
                      </div>

                      <div class="flex flex-wrap items-center justify-end gap-2">
                        <div class="flex flex-wrap items-center">
                          @if (comment.isEdited) {
                            <p-tag value="Edited" icon="pi pi-pencil" severity="warn"/>
                          }
                        </div>
                        @if (comment.canEdit) {
                          <p-button
                            icon="pi pi-pencil"
                            severity="secondary"
                            [rounded]="true"
                            [text]="true"
                            (onClick)="startEdit(comment)"
                          />
                        }
                        @if (comment.historyCount > 0) {
                          <p-button
                            icon="pi pi-history"
                            severity="secondary"
                            [rounded]="true"
                            [text]="true"
                            (onClick)="toggleHistory(comment)"
                          />
                        }
                        @if (comment.canDelete) {
                          <p-button
                            icon="pi pi-trash"
                            severity="danger"
                            [rounded]="true"
                            [text]="true"
                            (onClick)="deleteComment(comment)"
                          />
                        }
                      </div>
                    </div>
                    @if (editingCommentId === comment.commentId) {
                      <div class="flex flex-col gap-4">
                        <textarea
                          pTextarea
                          class="min-h-24 w-full"
                          [autoResize]="true"
                          rows="4"
                          [(ngModel)]="editContent"
                          placeholder="Edit comment"
                        ></textarea>
                        <div class="flex flex-col gap-2 sm:flex-row sm:justify-end">
                          <div>
                            <label class="file-trigger">
                              <p-button
                                label="Add attachment"
                                icon="pi pi-paperclip"
                                [text]="true"
                                severity="secondary"
                              />
                              <input type="file" multiple (change)="onInlineFilesSelected(comment, $event)"/>
                            </label>
                          </div>

                          <p-button
                            label="Save"
                            icon="pi pi-check"
                            [disabled]="!editContent.trim()"
                            (onClick)="saveEdit(comment)"
                          />
                          <p-button
                            label="Cancel"
                            icon="pi pi-times"
                            severity="secondary"
                            [text]="true"
                            (onClick)="cancelEdit()"
                          />
                        </div>
                      </div>
                    } @else {
                      <div class="markdown-preview" [innerHTML]="renderMarkdown(comment.content)"></div>
                    }

                    @if (comment.attachments.length > 0) {
                      <div class="flex flex-col gap-3">
                        <div class="flex flex-col gap-3">
                          @for (attachment of comment.attachments; track attachment.attachmentId) {
                            <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-end">
                              <div class="inline-flex items-center gap-2">
                                <i class="pi pi-file"></i>
                                <button
                                  type="button"
                                  class="break-all bg-transparent p-0 text-left font-semibold text-primary-600 transition hover:text-primary-500"
                                  (click)="downloadAttachment(attachment)"
                                >
                                  {{ attachment.fileName }}
                                </button>
                                <p-tag [value]="formatBytes(attachment.fileSize)" severity="contrast"/>
                              </div>

                              @if (comment.canEdit) {
                                <p-button
                                  icon="pi pi-times"
                                  severity="danger"
                                  [rounded]="true"
                                  [text]="true"
                                  (onClick)="deleteAttachment(comment, attachment)"
                                />
                              }
                            </div>
                          }
                        </div>
                      </div>
                    }

                    <div class="flex flex-wrap items-center gap-2">
                      @for (reaction of comment.reactions; track reaction.emojiCode) {
                        <p-button
                          [label]="reaction.emojiCode + ' ' + reaction.count"
                          severity="secondary"
                          [outlined]="!reaction.reactedByCurrentUser"
                          styleClass="reaction-button"
                          (onClick)="toggleReaction(comment, reaction)"
                        />
                      }

                      @for (emoji of emojiPalette; track emoji) {
                        <p-button
                          [label]="emoji"
                          severity="contrast"
                          [text]="true"
                          styleClass="reaction-button quick-reaction"
                          (onClick)="reactWithEmoji(comment, emoji)"
                        />
                      }
                    </div>

                    @if (historyByComment[comment.commentId]?.opened) {
                      <p-panel class="block" [toggleable]="true" [collapsed]="false">
                        <ng-template pTemplate="header">
                          <div class="inline-flex items-center gap-2 text-sm font-medium">
                            <i class="pi pi-history"></i>
                            <span>Edit history</span>
                          </div>
                        </ng-template>

                        @if (historyByComment[comment.commentId]?.loading) {
                          <div class="flex items-center gap-3 text-surface-500">
                            <p-progressSpinner strokeWidth="4" styleClass="history-spinner"/>
                            <span>Loading history...</span>
                          </div>
                        } @else {
                          <div class="flex flex-col gap-4">
                            @for (item of historyByComment[comment.commentId]?.items ?? []; track item.commentEditHistoryId) {
                              <div class="rounded-2xl border border-surface-200 px-4 py-4 dark:border-surface-700">
                                <div class="mb-3 inline-flex items-center gap-2 text-sm font-semibold">
                                  <i class="pi pi-clock"></i>
                                  <span>{{ item.editedByDisplayName }} · {{ item.editedAt | date: 'short' }}</span>
                                </div>
                                <div class="grid gap-4 lg:grid-cols-2">
                                  <div class="flex flex-col gap-2">
                                    <p-tag value="Before" severity="secondary"/>
                                    <div
                                      class="history-markdown rounded-2xl bg-surface-50 px-4 py-3 leading-7 dark:bg-surface-900/40"
                                      [innerHTML]="renderMarkdown(item.oldContent)"
                                    ></div>
                                  </div>
                                  <div class="flex flex-col gap-2">
                                    <p-tag value="After" severity="success"/>
                                    <div
                                      class="history-markdown rounded-2xl bg-surface-50 px-4 py-3 leading-7 dark:bg-surface-900/40"
                                      [innerHTML]="renderMarkdown(item.newContent)"
                                    ></div>
                                  </div>
                                </div>
                              </div>
                            }
                          </div>
                        }
                      </p-panel>
                    }
                  </div>
                </div>
              </ng-template>
            </p-card>
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

      .file-trigger {
        position: relative;
        display: inline-flex;
      }

      .file-trigger input {
        position: absolute;
        inset: 0;
        opacity: 0;
        cursor: pointer;
      }

      .markdown-preview,
      .history-markdown {
        word-break: break-word;
      }

      .markdown-preview :is(h1, h2, h3),
      .history-markdown :is(h1, h2, h3) {
        margin: 0 0 0.75rem;
        font-weight: 600;
      }

      .markdown-preview a,
      .history-markdown a {
        color: #2563eb;
        text-decoration: underline;
        text-underline-offset: 0.18em;
      }
    `
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DiscussionComponent implements OnChanges, OnDestroy, OnInit {

  private readonly discussionApi = inject(DiscussionApi);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly cdr = inject(ChangeDetectorRef);

  @Input({ required: true }) objectTypeId = 1;
  @Input({ required: true }) objectId = 1;

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
  emojiPalette = ['👍', '👎', '🐳', '🍆', '🍑'];
  private mentionSearchTimer: ReturnType<typeof setTimeout> | null = null;

  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['objectTypeId'] || changes['objectId']) && this.objectTypeId && this.objectId) {
      this.loadComments().then( );
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
        { content: this.editContent.trim() }
      );
      this.comments = this.comments.map((item) =>
        item.commentId === comment.commentId ? this.normalizeComment(updated) : item
      );
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
      return `${value} B`;
    }
    if (value < 1024 * 1024) {
      return `${(value / 1024).toFixed(1)} KB`;
    }
    return `${(value / (1024 * 1024)).toFixed(1)} MB`;
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

  private extractErrorMessage(error: unknown, fallback: string): string {
    if (typeof error === 'object' && error && 'message' in error && typeof error.message === 'string') {
      return error.message;
    }

    return fallback;
  }
}
