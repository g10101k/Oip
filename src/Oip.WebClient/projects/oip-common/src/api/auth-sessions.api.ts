/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import {
  ApiExceptionResponse,
  AuthSessionDto,
  DeleteAuthSessionParams,
  DeleteAuthSessionsByUserParams,
  DeleteAuthSessionsResponse,
  GetAuthSessionsParams,
} from "./data-contracts";
import { HttpClient, RequestParams } from "./http-client";

@Injectable()
export class AuthSessionsApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getAuthSessions = (
    query: GetAuthSessionsParams,
    params: RequestParams = {},
  ) =>
    this.request<AuthSessionDto[], ApiExceptionResponse>({
      path: `/api/auth-sessions/get-auth-sessions`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  deleteAuthSession = (
    { sessionId, ...query }: DeleteAuthSessionParams,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/auth-sessions/delete-auth-session/${sessionId}`,
      method: "DELETE",
      secure: true,
      ...params,
    });
  deleteAuthSessionsByUser = (
    { userId, ...query }: DeleteAuthSessionsByUserParams,
    params: RequestParams = {},
  ) =>
    this.request<DeleteAuthSessionsResponse, ApiExceptionResponse>({
      path: `/api/auth-sessions/delete-auth-sessions-by-user/${userId}`,
      method: "DELETE",
      secure: true,
      format: "json",
      ...params,
    });
  deleteAllAuthSessions = (params: RequestParams = {}) =>
    this.request<DeleteAuthSessionsResponse, ApiExceptionResponse>({
      path: `/api/auth-sessions/delete-all-auth-sessions`,
      method: "DELETE",
      secure: true,
      format: "json",
      ...params,
    });
}
