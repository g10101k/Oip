import { Injectable } from "@angular/core";
import {
  ApiExceptionResponse,
  ExistModuleDto,
  ModuleDeleteRequest,
  ModuleDto,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class ModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Retrieves all modules stored in the system.
   *
   * @tags Module
   * @name getAll
   * @summary Retrieves all modules stored in the system.
   * @request GET:/api/module/get-all
   * @secure
   * @response `200` `(ModuleDto)[]` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  getAll = (params: RequestParams = {}) =>
    this.request<ModuleDto[], ApiExceptionResponse>({
      path: `/api/module/get-all`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Inserts a new module into the system.
   *
   * @tags Module
   * @name insert
   * @summary Inserts a new module into the system.
   * @request POST:/api/module/insert
   * @secure
   * @response `200` `void` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  insert = (data: ModuleDto, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/module/insert`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @description Deletes a module by its identifier.
   *
   * @tags Module
   * @name delete
   * @summary Deletes a module by its identifier.
   * @request DELETE:/api/module/delete
   * @secure
   * @response `200` `void` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  delete = (data: ModuleDeleteRequest, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/module/delete`,
      method: "DELETE",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @description Returns all registered modules and indicates whether each one is currently loaded into the application.
   *
   * @tags Module
   * @name getModulesWithLoadStatus
   * @summary Returns all registered modules and indicates whether each one is currently loaded into the application.
   * @request GET:/api/module/get-modules-with-load-status
   * @secure
   * @response `200` `(ExistModuleDto)[]` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  getModulesWithLoadStatus = (params: RequestParams = {}) =>
    this.request<ExistModuleDto[], ApiExceptionResponse>({
      path: `/api/module/get-modules-with-load-status`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
