/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import {
  ApiExceptionResponse,
  GetNotificationByIdParams,
  GetNotificationByUserParams,
  UserNotificationCountResponse,
  UserNotificationDto,
  UserNotificationListResponse,
} from "./data-contracts";
import { HttpClient, RequestParams } from "./http-client";

@Injectable()
export class NotificationApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Gets notifications for the current user.
   *
   * @tags Notification
   * @name getNotificationByUser
   * @summary Gets notifications for the current user.
   * @request GET:/api/notification/get-notification-by-user
   * @secure
   * @response `200` `UserNotificationListResponse` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `404` `ApiExceptionResponse` Not Found
   * @response `500` `ApiExceptionResponse` Internal Server Error
   */
  getNotificationByUser = (
    query: GetNotificationByUserParams,
    params: RequestParams = {},
  ) =>
    this.request<UserNotificationListResponse, ApiExceptionResponse>({
      path: `/api/notification/get-notification-by-user`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Gets the current user notification count.
   *
   * @tags Notification
   * @name getNotificationCountByUser
   * @summary Gets the current user notification count.
   * @request GET:/api/notification/get-notification-count-by-user
   * @secure
   * @response `200` `UserNotificationCountResponse` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `404` `ApiExceptionResponse` Not Found
   * @response `500` `ApiExceptionResponse` Internal Server Error
   */
  getNotificationCountByUser = (params: RequestParams = {}) =>
    this.request<UserNotificationCountResponse, ApiExceptionResponse>({
      path: `/api/notification/get-notification-count-by-user`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Gets a current user notification by identifier.
   *
   * @tags Notification
   * @name getNotificationById
   * @summary Gets a current user notification by identifier.
   * @request GET:/api/notification/get-notification-by-id
   * @secure
   * @response `200` `UserNotificationDto` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `404` `ApiExceptionResponse` Not Found
   * @response `500` `ApiExceptionResponse` Internal Server Error
   */
  getNotificationById = (
    query: GetNotificationByIdParams,
    params: RequestParams = {},
  ) =>
    this.request<UserNotificationDto, ApiExceptionResponse>({
      path: `/api/notification/get-notification-by-id`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
}
