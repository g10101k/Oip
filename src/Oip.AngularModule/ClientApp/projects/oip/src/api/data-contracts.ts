export interface ApiExceptionResponse {
  title?: string | null;
  message?: string | null;
  statusCode?: number;
  stackTrace?: string | null;
}

export interface ExternalModuleExampleDataDto {
  message?: string | null;
  generatedAt?: Date;
  items?: string[] | null;
}

export interface ExternalModuleExampleModuleSettings {
  dayCount?: number;
}

export interface GetModuleInstanceSettingsParams {
  id?: number;
}
