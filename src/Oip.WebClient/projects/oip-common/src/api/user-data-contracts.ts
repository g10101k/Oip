export interface ApiExceptionResponse {
  title?: string | null;
  message?: string | null;
  statusCode?: number;
  stackTrace?: string | null;
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

export interface GetUserPhotoParams {
  email?: string;
}

export interface PostUserPhotoPayload {
  files?: File;
}
