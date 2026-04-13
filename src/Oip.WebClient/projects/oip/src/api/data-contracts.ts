export enum DemoCustomerStatus {
  Prospect = "Prospect",
  Active = "Active",
  Suspended = "Suspended",
}

/** Request for adding or toggling a reaction for a comment. */
export interface AddReactionRequest {
  /**
   * Comment identifier.
   * @format int64
   */
  commentId?: number;
  /**
   * Emoji code for the reaction.
   * @minLength 1
   * @maxLength 20
   */
  emojiCode: string;
}

/** Standardized response format for API exceptions */
export interface ApiExceptionResponse {
  /** User-friendly title of the exception */
  title?: string | null;
  /** Detailed description of the error */
  message?: string | null;
  /**
   * HTTP status code associated with the exception
   * @format int32
   */
  statusCode?: number;
  /** Stack trace information (optional, typically omitted in production) */
  stackTrace?: string | null;
}

/** Attachment response DTO. */
export interface AttachmentDto {
  /**
   * Attachment identifier.
   * @format int64
   */
  attachmentId?: number;
  /** Original file name. */
  fileName?: string | null;
  /** MIME type of the file. */
  fileType?: string | null;
  /**
   * File size in bytes.
   * @format int64
   */
  fileSize?: number;
  /**
   * Upload timestamp.
   * @format date-time
   */
  uploadedAt?: Date;
  /**
   * File storage identifier.
   * @format uuid
   */
  storageFileId?: string;
  /** Download URL for the file. */
  downloadUrl?: string | null;
}

/** Comment response DTO. */
export interface CommentDto {
  /**
   * Comment identifier.
   * @format int64
   */
  commentId?: number;
  /**
   * Related entity type identifier.
   * @format int64
   */
  objectTypeId?: number;
  /**
   * Related entity identifier.
   * @format int64
   */
  objectId?: number;
  /** Raw markdown comment content. */
  content?: string | null;
  /**
   * Author identifier.
   * @format int64
   */
  userId?: number;
  /** Author display name. */
  authorDisplayName?: string | null;
  /** Author e-mail. */
  authorEmail?: string | null;
  /**
   * Comment creation time.
   * @format date-time
   */
  createdAt?: Date;
  /**
   * Comment last update time.
   * @format date-time
   */
  updatedAt?: Date | null;
  /** Whether the comment was edited. */
  isEdited?: boolean;
  /**
   * Number of edit history entries.
   * @format int32
   */
  historyCount?: number;
  /** Whether the current user can edit this comment. */
  canEdit?: boolean;
  /** Whether the current user can delete this comment. */
  canDelete?: boolean;
  /** Attachments for the comment. */
  attachments?: AttachmentDto[] | null;
  /** Reactions aggregated for the comment. */
  reactions?: CommentReactionDto[] | null;
  /** Mentions for the comment. */
  mentions?: CommentMentionDto[] | null;
}

/** Comment edit history response DTO. */
export interface CommentHistoryDto {
  /**
   * History entry identifier.
   * @format int64
   */
  commentEditHistoryId?: number;
  /** Previous comment content. */
  oldContent?: string | null;
  /** Updated comment content. */
  newContent?: string | null;
  /**
   * Editor identifier.
   * @format int64
   */
  editedByUserId?: number;
  /** Editor display name. */
  editedByDisplayName?: string | null;
  /**
   * Time of edit.
   * @format date-time
   */
  editedAt?: Date;
}

/** Mention response DTO. */
export interface CommentMentionDto {
  /**
   * Mentioned user identifier.
   * @format int64
   */
  mentionedUserId?: number;
  /** Display name of the mentioned user. */
  displayName?: string | null;
  /** E-mail of the mentioned user. */
  email?: string | null;
  /**
   * Position within the markdown text.
   * @format int32
   */
  position?: number;
}

/** Aggregated reaction response DTO. */
export interface CommentReactionDto {
  /** Emoji code. */
  emojiCode?: string | null;
  /**
   * Number of reactions with the same emoji.
   * @format int32
   */
  count?: number;
  /** Whether the current user reacted with this emoji. */
  reactedByCurrentUser?: boolean;
}

/** Request for creating a new comment. */
export interface CreateCommentRequest {
  /**
   * Related entity type identifier.
   * @format int64
   */
  objectTypeId?: number;
  /**
   * Related entity identifier.
   * @format int64
   */
  objectId?: number;
  /**
   * Raw markdown comment content.
   * @minLength 1
   * @maxLength 4000
   */
  content: string;
}

/** Module settings for the customer module. */
export type CustomerModuleSettings = object;

/** Represents a request to save module instance settings. */
export interface CustomerModuleSettingsSaveSettingsRequest {
  /**
   * Gets or sets the ID of the module instance.
   * @format int32
   */
  id?: number;
  /** Module settings for the customer module. */
  settings?: CustomerModuleSettings;
}

/** Settings */
export interface DashboardSettings {
  /** Just, for example */
  nothing?: string | null;
}

/** Represents a request to save module instance settings. */
export interface DashboardSettingsSaveSettingsRequest {
  /**
   * Gets or sets the ID of the module instance.
   * @format int32
   */
  id?: number;
  /** Settings */
  settings?: DashboardSettings;
}

export interface DemoCustomerTableRowDto {
  /** @format int32 */
  id?: number;
  fullName?: string | null;
  email?: string | null;
  category?: string | null;
  country?: string | null;
  status?: DemoCustomerStatus;
  isActive?: boolean;
  /** @format int32 */
  creditScore?: number;
  /** @format double */
  lifetimeValue?: number;
  /** @format date-time */
  createdAt?: Date;
  /** @format int32 */
  ordersCount?: number;
}

/** Standard response for server-side table data requests. */
export interface DemoCustomerTableRowDtoTablePageResult {
  data?: DemoCustomerTableRowDto[] | null;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  first?: number;
  /** @format int32 */
  rows?: number;
}

/** Mention candidate response DTO. */
export interface MentionUserDto {
  /**
   * User identifier.
   * @format int64
   */
  userId?: number;
  /** Display name. */
  displayName?: string | null;
  /** E-mail. */
  email?: string | null;
}

/** Put security dto */
export interface PutSecurityRequest {
  /**
   * Instance id
   * @format int32
   */
  id?: number;
  /** Securities */
  securities?: SecurityResponse[] | null;
}

/** Request for creating or updating a demo customer. */
export interface SaveDemoCustomerRequest {
  /**
   * Full customer name.
   * @minLength 1
   * @maxLength 200
   */
  fullName: string;
  /**
   * Customer email.
   * @format email
   * @minLength 1
   * @maxLength 200
   */
  email: string;
  /**
   * Customer category name.
   * @minLength 1
   * @maxLength 100
   */
  category: string;
  /**
   * Customer country name.
   * @minLength 1
   * @maxLength 100
   */
  country: string;
  status: DemoCustomerStatus;
  /**
   * Credit score.
   * @format int32
   * @min 0
   * @max 1000
   */
  creditScore?: number;
  /**
   * Lifetime value.
   * @format double
   * @min 0
   */
  lifetimeValue?: number;
}

/** Security dto */
export interface SecurityResponse {
  /** Code */
  code?: string | null;
  /** Name */
  name?: string | null;
  /** Description */
  description?: string | null;
  /** Roles */
  roles?: string[] | null;
}

/** Represents a request to synchronize a user from Keycloak */
export interface SyncUserRequest {
  /** The unique identifier of the user in Keycloak */
  keycloakUserId?: string | null;
}

/** Represents a server-side table query request coming from PrimeNG p-table lazy loading. */
export interface TableQueryRequest {
  /**
   * Number of records to skip from the beginning of the result set.
   * @format int32
   */
  first?: number;
  /**
   * Number of rows to return.
   * @format int32
   */
  rows?: number;
  /** Requested field for sorting. */
  sortField?: string | null;
  /**
   * Sort direction, where values less than zero mean descending.
   * @format int32
   */
  sortOrder?: number;
  /** Optional global filter value applied across configured fields. */
  globalFilter?: string | null;
  /** Per-column filters keyed by field name. */
  filters?: Record<string, any>;
}

/** Request for updating an existing comment. */
export interface UpdateCommentRequest {
  /**
   * Raw markdown comment content.
   * @minLength 1
   * @maxLength 4000
   */
  content: string;
}

/** User entity */
export interface UserEntity {
  /**
   * User id
   * @format int32
   */
  userId?: number;
  /** Gets or sets the Keycloak identifier for the user. */
  keycloakId?: string | null;
  /** E-mail */
  email: string | null;
  /** First name */
  firstName?: string | null;
  /** Last name */
  lastName?: string | null;
  /** Indicates whether the user is active */
  isActive?: boolean;
  /**
   * Creation date and time
   * @format date-time
   */
  createdAt?: Date;
  /**
   * Last update date and time
   * @format date-time
   */
  updatedAt?: Date;
  /**
   * Last synchronization date and time
   * @format date-time
   */
  lastSyncedAt?: Date;
  /**
   * User photo
   * @format byte
   */
  photo?: string | null;
  /** User settings in json */
  settings?: string | null;
}

/** Response */
export interface WeatherForecastResponse {
  /**
   * Date
   * @format date-time
   */
  date?: Date;
  /**
   * Temp in ºC
   * @format int32
   */
  temperatureC?: number;
  /**
   * Temp in ºF
   * @format int32
   */
  temperatureF?: number;
  /** Summary */
  summary?: string | null;
}

/** Module settings */
export interface WeatherModuleSettings {
  /**
   * Day count
   * @format int32
   */
  dayCount?: number;
}

/** Represents a request to save module instance settings. */
export interface WeatherModuleSettingsSaveSettingsRequest {
  /**
   * Gets or sets the ID of the module instance.
   * @format int32
   */
  id?: number;
  /** Module settings */
  settings?: WeatherModuleSettings;
}

export interface UpdateParams {
  /** @format int32 */
  id: number;
}

export interface DeleteParams {
  /** @format int32 */
  id: number;
}

export interface GetSecurityParams {
  /**
   * The ID of the module instance.
   * @format int32
   */
  id?: number;
}

export interface GetModuleInstanceSettingsParams {
  /**
   * The ID of the module instance.
   * @format int32
   */
  id?: number;
}

export interface DashboardGetSecurityParams {
  /**
   * The ID of the module instance.
   * @format int32
   */
  id?: number;
}

export interface DashboardGetModuleInstanceSettingsParams {
  /**
   * The ID of the module instance.
   * @format int32
   */
  id?: number;
}

export interface GetByObjectParams {
  /** @format int64 */
  objectTypeId?: number;
  /** @format int64 */
  objectId?: number;
  /**
   * @format int32
   * @default 0
   */
  skip?: number;
  /**
   * @format int32
   * @default 50
   */
  take?: number;
}

export interface GetByIdParams {
  /** @format int64 */
  id?: number;
}

export interface UpdateParams2 {
  /** @format int64 */
  id: number;
}

export interface DeleteParams2 {
  /** @format int64 */
  id: number;
}

export interface GetHistoryParams {
  /** @format int64 */
  id: number;
}

export interface UploadAttachmentPayload {
  /**
   * Comment identifier.
   * @format int64
   */
  CommentId: number;
  /**
   * Attached file.
   * @format binary
   */
  File: File;
}

export interface DeleteAttachmentParams {
  /** @format int64 */
  id: number;
}

export interface GetAttachmentContentParams {
  /** @format int64 */
  id: number;
}

export interface RemoveReactionParams {
  /** @format int64 */
  commentId?: number;
  emojiCode?: string;
}

export interface SearchMentionUsersParams {
  term?: string;
}

export interface GetAllUsersParams {
  /**
   * Number of records to skip
   * @format int32
   * @default 0
   */
  skip?: number;
  /**
   * Number of records to take
   * @format int32
   * @default 100
   */
  take?: number;
}

export interface GetUserParams {
  /**
   * User ID
   * @format int32
   */
  id?: number;
}

export interface GetUserByKeycloakIdParams {
  /** Keycloak user ID */
  keycloakId?: string;
}

export interface SearchUserParams {
  /** Search term */
  term?: string;
}

export interface GetWeatherForecastParams {
  /** @format int32 */
  dayCount?: number;
}

export interface GetSecurityParams2 {
  /**
   * The ID of the module instance.
   * @format int32
   */
  id?: number;
}

export interface GetModuleInstanceSettingsParams2 {
  /**
   * The ID of the module instance.
   * @format int32
   */
  id?: number;
}
