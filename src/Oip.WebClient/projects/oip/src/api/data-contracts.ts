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

export interface GetWeatherForecastParams {
  /** @format int32 */
  dayCount?: number;
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
