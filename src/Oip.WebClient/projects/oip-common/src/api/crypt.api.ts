import { Injectable } from "@angular/core";
import { ApiExceptionResponse, CryptRequest } from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class CryptApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Protects a message using encryption services.
   *
   * @tags Crypt
   * @name protect
   * @summary Protects a message using encryption services.
   * @request POST:/api/crypt/protect
   * @secure
   * @response `200` `string` OK
   * @response `500` `ApiExceptionResponse` Internal Server Error
   */
  protect = (data: CryptRequest, params: RequestParams = {}) =>
    this.request<string, ApiExceptionResponse>({
      path: `/api/crypt/protect`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}
