export enum ImportanceLevel {
  Unspecified = "Unspecified",
  Low = "Low",
  Medium = "Medium",
  High = "High",
  Critical = "Critical",
}

export interface ApiExceptionResponse {
  title?: string | null;
  message?: string | null;
  statusCode?: number;
  stackTrace?: string | null;
}

export interface UserNotificationCountResponse {
  count?: number;
}

export interface UserNotificationDto {
  notificationUserId?: number;
  notificationId?: number;
  notificationTypeId?: number;
  notificationTypeName?: string | null;
  subject?: string | null;
  message?: string | null;
  importance?: ImportanceLevel;
  notificationChannelId?: number | null;
  sentAt?: Date | null;
  deliveredAt?: Date | null;
  readAt?: Date | null;
  createdAt?: Date;
  dataJson?: string | null;
}

export interface UserNotificationListResponse {
  notifications?: UserNotificationDto[] | null;
  totalCount?: number;
}

export interface GetNotificationByUserParams {
  skip?: number;
  take?: number;
  unreadOnly?: boolean;
}

export interface MarkNotificationAsReadParams {
  id: number;
}

export interface GetNotificationByIdParams {
  id?: number;
}
