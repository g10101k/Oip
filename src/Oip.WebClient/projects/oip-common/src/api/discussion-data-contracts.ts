export interface AddReactionRequest {
  commentId?: number;
  emojiCode: string;
}

export interface ApiExceptionResponse {
  title?: string | null;
  message?: string | null;
  statusCode?: number;
  stackTrace?: string | null;
}

export interface AttachmentDto {
  attachmentId?: number;
  fileName?: string | null;
  fileType?: string | null;
  fileSize?: number;
  uploadedAt?: Date;
  storageFileId?: string;
  downloadUrl?: string | null;
}

export interface CommentDto {
  commentId?: number;
  objectTypeId?: number;
  objectId?: number;
  content?: string | null;
  userId?: number;
  authorDisplayName?: string | null;
  authorEmail?: string | null;
  createdAt?: Date;
  updatedAt?: Date | null;
  isEdited?: boolean;
  historyCount?: number;
  canEdit?: boolean;
  canDelete?: boolean;
  attachments?: AttachmentDto[] | null;
  reactions?: CommentReactionDto[] | null;
  mentions?: CommentMentionDto[] | null;
}

export interface CommentHistoryDto {
  commentEditHistoryId?: number;
  oldContent?: string | null;
  newContent?: string | null;
  editedByUserId?: number;
  editedByDisplayName?: string | null;
  editedAt?: Date;
}

export interface CommentMentionDto {
  mentionedUserId?: number;
  displayName?: string | null;
  email?: string | null;
  position?: number;
}

export interface CommentReactionDto {
  emojiCode?: string | null;
  count?: number;
  reactedByCurrentUser?: boolean;
}

export interface CreateCommentRequest {
  objectTypeId?: number;
  objectId?: number;
  content: string;
}

export interface MentionUserDto {
  userId?: number;
  displayName?: string | null;
  email?: string | null;
}

export interface UpdateCommentRequest {
  content: string;
}

export interface GetByObjectParams {
  objectTypeId?: number;
  objectId?: number;
  skip?: number;
  take?: number;
}

export interface GetByIdParams {
  id?: number;
}

export interface UpdateParams {
  id: number;
}

export interface DeleteParams {
  id: number;
}

export interface GetHistoryParams {
  id: number;
}

export interface UploadAttachmentPayload {
  CommentId: number;
  File: Blob;
}

export interface DeleteAttachmentParams {
  id: number;
}

export interface GetAttachmentContentParams {
  id: number;
}

export interface RemoveReactionParams {
  commentId?: number;
  emojiCode?: string;
}

export interface SearchMentionUsersParams {
  term?: string;
}
