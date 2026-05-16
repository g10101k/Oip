/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { HttpClient, RequestParams } from "./http-client";
import {
  ApiExceptionResponse,
  GetNotificationByIdParams,
  GetNotificationByUserParams,
  MarkNotificationAsReadParams,
  UserNotificationCountResponse,
  UserNotificationDto,
  UserNotificationListResponse,
} from "./notification-data-contracts";

@Injectable()
export class NotificationApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
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
  getNotificationCountByUser = (params: RequestParams = {}) =>
    this.request<UserNotificationCountResponse, ApiExceptionResponse>({
      path: `/api/notification/get-notification-count-by-user`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  markNotificationAsRead = (
    { id, ...query }: MarkNotificationAsReadParams,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/notification/mark-notification-as-read/${id}`,
      method: "POST",
      secure: true,
      ...params,
    });
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
