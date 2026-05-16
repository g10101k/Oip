export interface AddModuleInstanceDto {
  moduleId?: number;
  label?: string | null;
  icon?: string | null;
  parentId?: number | null;
  viewRoles?: string[] | null;
}

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

export interface AuthCsrfTokenResponse {
  token?: string | null;
  headerName?: string | null;
}

export interface AuthSessionResponse {
  isAuthenticated?: boolean;
  userName?: string | null;
  displayName?: string | null;
  email?: string | null;
  roles?: string[] | null;
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

export interface CryptRequest {
  message?: string | null;
}

export interface EditModuleInstanceDto {
  moduleInstanceId?: number;
  label?: string | null;
  icon?: string | null;
  parentId?: number | null;
  viewRoles?: string[] | null;
  moduleId?: number | null;
}

export interface ExistModuleDto {
  moduleId?: number;
  name?: string | null;
  currentlyLoaded?: boolean;
}

export interface FolderModuleSettings {
  html?: string | null;
}

export interface GetKeycloakClientSettingsResponse {
  authority?: string | null;
  clientId?: string | null;
  scope?: string | null;
  responseType?: string | null;
  useRefreshToken?: boolean;
  silentRenew?: boolean;
  logLevel?: number;
  secureRoutes?: string[] | null;
}

export interface IframeModuleSettings {
  url?: string | null;
}

export interface IntKeyValueDto {
  key?: number;
  value?: string | null;
}

export interface MentionUserDto {
  userId?: number;
  displayName?: string | null;
  email?: string | null;
}

export interface ModuleDeleteRequest {
  moduleId?: number;
}

export interface ModuleDto {
  moduleId?: number;
  name?: string | null;
  settings?: string | null;
  moduleSecurities?: ModuleSecurityDto[] | null;
}

export interface ModuleInstanceDto {
  moduleInstanceId?: number;
  moduleId?: number;
  label?: string | null;
  icon?: string | null;
  routerLink?: string[] | null;
  url?: string | null;
  target?: string | null;
  settings?: string | null;
  items?: ModuleInstanceDto[] | null;
  securities?: string[] | null;
  parentId?: number | null;
  order?: number;
  separator?: boolean;
}

export interface ModuleSecurityDto {
  right: string | null;
  role: string | null;
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
  File: File;
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

export interface GetModuleInstanceSettingsParams {
  id?: number;
}

export interface GetModuleInstanceSettingsParams2 {
  id?: number;
}

export interface DeleteModuleInstanceParams {
  id?: number;
}

export interface ChangeOrderParams {
  firstModuleId?: number;
  secondModuleId?: number;
}
