/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

/** DTO for create module instance */
export interface AddModuleInstanceDto {
  /** @format int32 */
  moduleId?: number;
  label?: string | null;
  icon?: string | null;
  /** @format int32 */
  parentId?: number | null;
  viewRoles?: string[] | null;
}

/** Apply Migration Request */
export interface ApplyMigrationRequest {
  /** Migration name */
  name?: string | null;
}

/** Settings */
export interface DashboardSettings {
  /** Just for example */
  nothing?: string | null;
}

/** Save settings request */
export interface DashboardSettingsSaveSettingsRequest {
  /**
   * Module instance id
   * @format int32
   */
  id?: number;
  /** Settings */
  settings?: DashboardSettings;
}

/** DTO for edit module instance */
export interface EditModuleInstanceDto {
  /** @format int32 */
  moduleInstanceId?: number;
  label?: string | null;
  icon?: string | null;
  /** @format int32 */
  parentId?: number | null;
  viewRoles?: string[] | null;
}

/** Response front security settings */
export interface GetKeycloakClientSettingsResponse {
  /** Authority */
  authority?: string | null;
  /** Client id */
  clientId?: string | null;
  /** Scope */
  scope?: string | null;
  /** Response Type */
  responseType?: string | null;
  /** Use Refresh Token */
  useRefreshToken?: boolean;
  /** Silent Renew */
  silentRenew?: boolean;
  /**
   * Log level None = 0, Debug = 1, Warn = 2, Error = 3
   * @format int32
   */
  logLevel?: number;
  /** Urls with auth */
  secureRoutes?: string[] | null;
}

/** Response for module federation */
export interface GetManifestResponse {
  /** Base Url */
  baseUrl?: string | null;
}

/** Int Key Value DTO */
export interface IntKeyValueDto {
  /** @format int32 */
  key?: number;
  value?: string | null;
}

/** Модель миграции */
export interface MigrationDto {
  name?: string | null;
  applied?: boolean;
  pending?: boolean;
  exist?: boolean;
}

/** Represents a request to delete a module by its identifier. */
export interface ModuleDeleteRequest {
  /**
   * Gets or sets the unique identifier of the module to be deleted.
   * @format int32
   */
  moduleId?: number;
}

/** It module in app */
export interface ModuleDto {
  /**
   * Id
   * @format int32
   */
  moduleId?: number;
  /** Name */
  name?: string | null;
  /** Settings */
  settings?: string | null;
  /** Securities */
  moduleSecurities?: ModuleSecurityDto[] | null;
}

/** Module Instance Dto */
export interface ModuleInstanceDto {
  /** @format int32 */
  moduleInstanceId?: number;
  /** @format int32 */
  moduleId?: number;
  label?: string | null;
  icon?: string | null;
  routerLink?: string[] | null;
  url?: string | null;
  target?: string | null;
  settings?: string | null;
  /** Childs */
  items?: ModuleInstanceDto[] | null;
}

/** Module security DTO */
export interface ModuleSecurityDto {
  /** Right */
  right?: string | null;
  /** Role */
  role?: string | null;
}

/** Save settings request */
export interface ObjectSaveSettingsRequest {
  /**
   * Module instance id
   * @format int32
   */
  id?: number;
  /** Settings */
  settings?: any;
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

/** Dto module */
export interface RegisterModuleDto {
  /** See 'name' in webpack.config.js */
  name?: string | null;
  /** Base Url */
  baseUrl?: string | null;
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
  date?: string;
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

/** Save settings request */
export interface WeatherModuleSettingsSaveSettingsRequest {
  /**
   * Module instance id
   * @format int32
   */
  id?: number;
  /** Module settings */
  settings?: WeatherModuleSettings;
}

export interface UserProfilePostUserPhotoCreatePayload {
  /** @format binary */
  files?: File;
}

export type QueryParamsType = Record<string | number, any>;
export type ResponseFormat = keyof Omit<Body, "body" | "bodyUsed">;

export interface FullRequestParams extends Omit<RequestInit, "body"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseFormat;
  /** request body */
  body?: unknown;
  /** base url */
  baseUrl?: string;
  /** request cancellation token */
  cancelToken?: CancelToken;
}

export type RequestParams = Omit<
  FullRequestParams,
  "body" | "method" | "query" | "path"
>;

export interface ApiConfig<SecurityDataType = unknown> {
  baseUrl?: string;
  baseApiParams?: Omit<RequestParams, "baseUrl" | "cancelToken" | "signal">;
  securityWorker?: (
    securityData: SecurityDataType | null,
  ) => Promise<RequestParams | void> | RequestParams | void;
  customFetch?: typeof fetch;
}

export interface HttpResponse<D extends unknown, E extends unknown = unknown>
  extends Response {
  data: D;
  error: E;
}

type CancelToken = Symbol | string | number;

export enum ContentType {
  Json = "application/json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public baseUrl: string = "";
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private abortControllers = new Map<CancelToken, AbortController>();
  private customFetch = (...fetchParams: Parameters<typeof fetch>) =>
    fetch(...fetchParams);

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {},
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor(apiConfig: ApiConfig<SecurityDataType> = {}) {
    Object.assign(this, apiConfig);
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected encodeQueryParam(key: string, value: any) {
    const encodedKey = encodeURIComponent(key);
    return `${encodedKey}=${encodeURIComponent(typeof value === "number" ? value : `${value}`)}`;
  }

  protected addQueryParam(query: QueryParamsType, key: string) {
    return this.encodeQueryParam(key, query[key]);
  }

  protected addArrayQueryParam(query: QueryParamsType, key: string) {
    const value = query[key];
    return value.map((v: any) => this.encodeQueryParam(key, v)).join("&");
  }

  protected toQueryString(rawQuery?: QueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter(
      (key) => "undefined" !== typeof query[key],
    );
    return keys
      .map((key) =>
        Array.isArray(query[key])
          ? this.addArrayQueryParam(query, key)
          : this.addQueryParam(query, key),
      )
      .join("&");
  }

  protected addQueryParams(rawQuery?: QueryParamsType): string {
    const queryString = this.toQueryString(rawQuery);
    return queryString ? `?${queryString}` : "";
  }

  private contentFormatters: Record<ContentType, (input: any) => any> = {
    [ContentType.Json]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.Text]: (input: any) =>
      input !== null && typeof input !== "string"
        ? JSON.stringify(input)
        : input,
    [ContentType.FormData]: (input: any) =>
      Object.keys(input || {}).reduce((formData, key) => {
        const property = input[key];
        formData.append(
          key,
          property instanceof Blob
            ? property
            : typeof property === "object" && property !== null
              ? JSON.stringify(property)
              : `${property}`,
        );
        return formData;
      }, new FormData()),
    [ContentType.UrlEncoded]: (input: any) => this.toQueryString(input),
  };

  protected mergeRequestParams(
    params1: RequestParams,
    params2?: RequestParams,
  ): RequestParams {
    return {
      ...this.baseApiParams,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected createAbortSignal = (
    cancelToken: CancelToken,
  ): AbortSignal | undefined => {
    if (this.abortControllers.has(cancelToken)) {
      const abortController = this.abortControllers.get(cancelToken);
      if (abortController) {
        return abortController.signal;
      }
      return void 0;
    }

    const abortController = new AbortController();
    this.abortControllers.set(cancelToken, abortController);
    return abortController.signal;
  };

  public abortRequest = (cancelToken: CancelToken) => {
    const abortController = this.abortControllers.get(cancelToken);

    if (abortController) {
      abortController.abort();
      this.abortControllers.delete(cancelToken);
    }
  };

  public request = async <T = any, E = any>({
    body,
    secure,
    path,
    type,
    query,
    format,
    baseUrl,
    cancelToken,
    ...params
  }: FullRequestParams): Promise<HttpResponse<T, E>> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.baseApiParams.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const queryString = query && this.toQueryString(query);
    const payloadFormatter = this.contentFormatters[type || ContentType.Json];
    const responseFormat = format || requestParams.format;

    return this.customFetch(
      `${baseUrl || this.baseUrl || ""}${path}${queryString ? `?${queryString}` : ""}`,
      {
        ...requestParams,
        headers: {
          ...(requestParams.headers || {}),
          ...(type && type !== ContentType.FormData
            ? { "Content-Type": type }
            : {}),
        },
        signal:
          (cancelToken
            ? this.createAbortSignal(cancelToken)
            : requestParams.signal) || null,
        body:
          typeof body === "undefined" || body === null
            ? null
            : payloadFormatter(body),
      },
    ).then(async (response) => {
      const r = response.clone() as HttpResponse<T, E>;
      r.data = null as unknown as T;
      r.error = null as unknown as E;

      const data = !responseFormat
        ? r
        : await response[responseFormat]()
            .then((data) => {
              if (r.ok) {
                r.data = data;
              } else {
                r.error = data;
              }
              return r;
            })
            .catch((e) => {
              r.error = e;
              return r;
            });

      if (cancelToken) {
        this.abortControllers.delete(cancelToken);
      }

      if (!response.ok) throw data;
      return data;
    });
  };
}

/**
 * @title Oip service web-api
 * @version v1.0.0
 *
 * Oip service web-api
 */
export class OipApi<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  api = {
    /**
     * No description
     *
     * @tags DashboardModule
     * @name dashboardGetModuleRightsList
     * @request GET:/api/dashboard/get-module-rights
     * @secure
     */
    dashboardGetModuleRightsList: (params: RequestParams = {}) =>
      this.request<SecurityResponse[], any>({
        path: `/api/dashboard/get-module-rights`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DashboardModule
     * @name dashboardGetSecurityList
     * @summary Get security for instance id
     * @request GET:/api/dashboard/get-security
     * @secure
     */
    dashboardGetSecurityList: (
      query?: {
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<SecurityResponse[], any>({
        path: `/api/dashboard/get-security`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DashboardModule
     * @name dashboardPutSecurityUpdate
     * @summary Update security
     * @request PUT:/api/dashboard/put-security
     * @secure
     */
    dashboardPutSecurityUpdate: (
      data: PutSecurityRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/dashboard/put-security`,
        method: "PUT",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags DashboardModule
     * @name dashboardGetModuleInstanceSettingsList
     * @summary Get instance setting
     * @request GET:/api/dashboard/get-module-instance-settings
     * @secure
     */
    dashboardGetModuleInstanceSettingsList: (
      query?: {
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/dashboard/get-module-instance-settings`,
        method: "GET",
        query: query,
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags DashboardModule
     * @name dashboardPutModuleInstanceSettingsUpdate
     * @request PUT:/api/dashboard/put-module-instance-settings
     * @secure
     */
    dashboardPutModuleInstanceSettingsUpdate: (
      data: DashboardSettingsSaveSettingsRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/dashboard/put-module-instance-settings`,
        method: "PUT",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags DataContextMigrationModule
     * @name dbMigrationGetModuleRightsList
     * @request GET:/api/db-migration/get-module-rights
     * @secure
     */
    dbMigrationGetModuleRightsList: (params: RequestParams = {}) =>
      this.request<SecurityResponse[], any>({
        path: `/api/db-migration/get-module-rights`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DataContextMigrationModule
     * @name dbMigrationGetMigrationsList
     * @summary Get migration
     * @request GET:/api/db-migration/get-migrations
     * @secure
     */
    dbMigrationGetMigrationsList: (params: RequestParams = {}) =>
      this.request<MigrationDto[], any>({
        path: `/api/db-migration/get-migrations`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DataContextMigrationModule
     * @name dbMigrationMigrateList
     * @summary Применить миграцию БД
     * @request GET:/api/db-migration/migrate
     * @secure
     */
    dbMigrationMigrateList: (params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/db-migration/migrate`,
        method: "GET",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags DataContextMigrationModule
     * @name dbMigrationApplyMigrationCreate
     * @summary Применить миграцию БД
     * @request POST:/api/db-migration/apply-migration
     * @secure
     */
    dbMigrationApplyMigrationCreate: (
      data: ApplyMigrationRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/db-migration/apply-migration`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags DataContextMigrationModule
     * @name dbMigrationGetSecurityList
     * @summary Get security for instance id
     * @request GET:/api/db-migration/get-security
     * @secure
     */
    dbMigrationGetSecurityList: (
      query?: {
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<SecurityResponse[], any>({
        path: `/api/db-migration/get-security`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DataContextMigrationModule
     * @name dbMigrationPutSecurityUpdate
     * @summary Update security
     * @request PUT:/api/db-migration/put-security
     * @secure
     */
    dbMigrationPutSecurityUpdate: (
      data: PutSecurityRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/db-migration/put-security`,
        method: "PUT",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags DataContextMigrationModule
     * @name dbMigrationGetModuleInstanceSettingsList
     * @summary Get instance setting
     * @request GET:/api/db-migration/get-module-instance-settings
     * @secure
     */
    dbMigrationGetModuleInstanceSettingsList: (
      query?: {
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/db-migration/get-module-instance-settings`,
        method: "GET",
        query: query,
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags DataContextMigrationModule
     * @name dbMigrationPutModuleInstanceSettingsUpdate
     * @request PUT:/api/db-migration/put-module-instance-settings
     * @secure
     */
    dbMigrationPutModuleInstanceSettingsUpdate: (
      data: ObjectSaveSettingsRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/db-migration/put-module-instance-settings`,
        method: "PUT",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags FolderModule
     * @name folderGetModuleRightsList
     * @request GET:/api/folder/get-module-rights
     * @secure
     */
    folderGetModuleRightsList: (params: RequestParams = {}) =>
      this.request<SecurityResponse[], any>({
        path: `/api/folder/get-module-rights`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags FolderModule
     * @name folderGetSecurityList
     * @summary Get security for instance id
     * @request GET:/api/folder/get-security
     * @secure
     */
    folderGetSecurityList: (
      query?: {
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<SecurityResponse[], any>({
        path: `/api/folder/get-security`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags FolderModule
     * @name folderPutSecurityUpdate
     * @summary Update security
     * @request PUT:/api/folder/put-security
     * @secure
     */
    folderPutSecurityUpdate: (
      data: PutSecurityRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/folder/put-security`,
        method: "PUT",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags FolderModule
     * @name folderGetModuleInstanceSettingsList
     * @summary Get instance setting
     * @request GET:/api/folder/get-module-instance-settings
     * @secure
     */
    folderGetModuleInstanceSettingsList: (
      query?: {
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/folder/get-module-instance-settings`,
        method: "GET",
        query: query,
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags FolderModule
     * @name folderPutModuleInstanceSettingsUpdate
     * @request PUT:/api/folder/put-module-instance-settings
     * @secure
     */
    folderPutModuleInstanceSettingsUpdate: (
      data: ObjectSaveSettingsRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/folder/put-module-instance-settings`,
        method: "PUT",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Menu
     * @name menuGetList
     * @summary Get menu for client app
     * @request GET:/api/menu/get
     * @secure
     */
    menuGetList: (params: RequestParams = {}) =>
      this.request<ModuleInstanceDto[], any>({
        path: `/api/menu/get`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Menu
     * @name menuGetAdminMenuList
     * @summary Get admin menu for client app
     * @request GET:/api/menu/get-admin-menu
     * @secure
     */
    menuGetAdminMenuList: (params: RequestParams = {}) =>
      this.request<ModuleInstanceDto[], any>({
        path: `/api/menu/get-admin-menu`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Menu
     * @name menuGetModulesList
     * @summary Get admin menu for client app
     * @request GET:/api/menu/get-modules
     * @secure
     */
    menuGetModulesList: (params: RequestParams = {}) =>
      this.request<IntKeyValueDto[], any>({
        path: `/api/menu/get-modules`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Menu
     * @name menuAddModuleInstanceCreate
     * @summary Add new module
     * @request POST:/api/menu/add-module-instance
     * @secure
     */
    menuAddModuleInstanceCreate: (
      data: AddModuleInstanceDto,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/menu/add-module-instance`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Menu
     * @name menuEditModuleInstanceCreate
     * @summary Add new module
     * @request POST:/api/menu/edit-module-instance
     * @secure
     */
    menuEditModuleInstanceCreate: (
      data: EditModuleInstanceDto,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/menu/edit-module-instance`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Menu
     * @name menuDeleteModuleInstanceDelete
     * @summary Add new module
     * @request DELETE:/api/menu/delete-module-instance
     * @secure
     */
    menuDeleteModuleInstanceDelete: (
      query?: {
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/menu/delete-module-instance`,
        method: "DELETE",
        query: query,
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Module
     * @name moduleGetAllList
     * @summary Get all modules
     * @request GET:/api/module/get-all
     * @secure
     */
    moduleGetAllList: (params: RequestParams = {}) =>
      this.request<ModuleDto[], any>({
        path: `/api/module/get-all`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Module
     * @name moduleInsertCreate
     * @summary Insert
     * @request POST:/api/module/insert
     * @secure
     */
    moduleInsertCreate: (data: ModuleDto, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/module/insert`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Module
     * @name moduleDeleteDelete
     * @summary delete
     * @request DELETE:/api/module/delete
     * @secure
     */
    moduleDeleteDelete: (
      data: ModuleDeleteRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/module/delete`,
        method: "DELETE",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * @description This endpoint is restricted to users with administrative privileges. It aggregates module data from the database and compares it against the currently loaded modules in the application context, returning a combined view with load status flags.
     *
     * @tags Module
     * @name moduleGetModulesWithLoadStatusList
     * @summary Returns a list of all registered modules and indicates whether each one is currently loaded into the application.
     * @request GET:/api/module/get-modules-with-load-status
     * @secure
     */
    moduleGetModulesWithLoadStatusList: (params: RequestParams = {}) =>
      this.request<ModuleDto[], any>({
        path: `/api/module/get-modules-with-load-status`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Security
     * @name securityGetRealmRolesList
     * @summary Get all roles
     * @request GET:/api/security/get-realm-roles
     * @secure
     */
    securityGetRealmRolesList: (params: RequestParams = {}) =>
      this.request<string[], any>({
        path: `/api/security/get-realm-roles`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Security
     * @name securityGetKeycloakClientSettingsList
     * @summary Get keycloak client settings
     * @request GET:/api/security/get-keycloak-client-settings
     * @secure
     */
    securityGetKeycloakClientSettingsList: (params: RequestParams = {}) =>
      this.request<GetKeycloakClientSettingsResponse, any>({
        path: `/api/security/get-keycloak-client-settings`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Service
     * @name serviceGetList
     * @summary Get manifest for client app
     * @request GET:/api/service/get
     * @secure
     */
    serviceGetList: (params: RequestParams = {}) =>
      this.request<Record<string, GetManifestResponse>, any>({
        path: `/api/service/get`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Service
     * @name serviceRegisterModuleCreate
     * @summary Registry module
     * @request POST:/api/service/register-module
     * @secure
     */
    serviceRegisterModuleCreate: (
      data: RegisterModuleDto,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/service/register-module`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags UserProfile
     * @name userProfileGetUserPhotoList
     * @summary Get all roles
     * @request GET:/api/user-profile/get-user-photo
     * @secure
     */
    userProfileGetUserPhotoList: (
      query?: {
        email?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/user-profile/get-user-photo`,
        method: "GET",
        query: query,
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags UserProfile
     * @name userProfilePostUserPhotoCreate
     * @summary Get all roles
     * @request POST:/api/user-profile/post-user-photo
     * @secure
     */
    userProfilePostUserPhotoCreate: (
      data: UserProfilePostUserPhotoCreatePayload,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/user-profile/post-user-photo`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.FormData,
        ...params,
      }),

    /**
     * No description
     *
     * @tags WeatherForecast
     * @name weatherGetList
     * @summary Get example data
     * @request GET:/api/weather/get
     * @secure
     */
    weatherGetList: (
      query?: {
        /** @format int32 */
        dayCount?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<WeatherForecastResponse[], any>({
        path: `/api/weather/get`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags WeatherForecast
     * @name weatherGetModuleRightsList
     * @summary <inheritdoc />
     * @request GET:/api/weather/get-module-rights
     * @secure
     */
    weatherGetModuleRightsList: (params: RequestParams = {}) =>
      this.request<SecurityResponse[], any>({
        path: `/api/weather/get-module-rights`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags WeatherForecast
     * @name weatherGetSecurityList
     * @summary Get security for instance id
     * @request GET:/api/weather/get-security
     * @secure
     */
    weatherGetSecurityList: (
      query?: {
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<SecurityResponse[], any>({
        path: `/api/weather/get-security`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags WeatherForecast
     * @name weatherPutSecurityUpdate
     * @summary Update security
     * @request PUT:/api/weather/put-security
     * @secure
     */
    weatherPutSecurityUpdate: (
      data: PutSecurityRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/weather/put-security`,
        method: "PUT",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags WeatherForecast
     * @name weatherGetModuleInstanceSettingsList
     * @summary Get instance setting
     * @request GET:/api/weather/get-module-instance-settings
     * @secure
     */
    weatherGetModuleInstanceSettingsList: (
      query?: {
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/weather/get-module-instance-settings`,
        method: "GET",
        query: query,
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags WeatherForecast
     * @name weatherPutModuleInstanceSettingsUpdate
     * @request PUT:/api/weather/put-module-instance-settings
     * @secure
     */
    weatherPutModuleInstanceSettingsUpdate: (
      data: WeatherModuleSettingsSaveSettingsRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/weather/put-module-instance-settings`,
        method: "PUT",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),
  };
}
