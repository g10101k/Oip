export interface ApiExceptionResponse {
  title?: string | null;
  message?: string | null;
  statusCode?: number;
  stackTrace?: string | null;
}

export interface CustomUserNotify {
  username?: string | null;
}

export interface GetUsersByFilterRequest {
  skip?: number;
  take?: number;
  search?: string | null;
  isActive?: boolean | null;
}

export interface SyncUserRequest {
  keycloakUserId?: string | null;
}

export interface UserEntity {
  userId?: number;
  keycloakId?: string | null;
  email: string | null;
  firstName?: string | null;
  lastName?: string | null;
  isActive?: boolean;
  createdAt?: Date;
  updatedAt?: Date;
  lastSyncedAt?: Date;
  photoObjectName?: string | null;
  photoContentType?: string | null;
  settings?: string | null;
}

export interface UserListItemDto {
  userId?: number;
  email?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  isActive?: boolean;
  lastSyncedAt?: Date;
}

export interface UserSettingsDto {
  preset?: string | null;
  primary?: string | null;
  surface?: string | null;
  darkTheme?: boolean;
  menuMode?: string | null;
  language?: string | null;
  dateFormat?: string | null;
  timeFormat?: string | null;
  timeZone?: string | null;
}

export interface UsersPageDto {
  users?: UserListItemDto[] | null;
  totalCount?: number;
}

export interface UsersSummaryDto {
  totalCount?: number;
  activeCount?: number;
  inactiveCount?: number;
}

export interface UsersSyncStatus {
  isRunning?: boolean;
  startedAt?: Date | null;
  finishedAt?: Date | null;
  syncedCount?: number | null;
  failed?: boolean;
}

export interface GetUserPhotoByIdParams {
  userId: number;
}

export interface PostUserPhotoPayload {
  files?: Blob;
}

export interface GetAllUsersParams {
  skip?: number;
  take?: number;
}

export interface GetUserParams {
  id?: number;
}

export interface GetUserByKeycloakIdParams {
  keycloakId?: string;
}

export interface SearchUserParams {
  term?: string;
}
