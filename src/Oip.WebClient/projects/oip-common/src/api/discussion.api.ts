/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import {
  AddReactionRequest,
  ApiExceptionResponse,
  AttachmentDto,
  CommentDto,
  CommentHistoryDto,
  CommentReactionDto,
  CreateCommentRequest,
  DeleteAttachmentParams,
  DeleteParams,
  GetAttachmentContentParams,
  GetByIdParams,
  GetByObjectParams,
  GetHistoryParams,
  MentionUserDto,
  RemoveReactionParams,
  SearchMentionUsersParams,
  UpdateCommentRequest,
  UpdateParams,
  UploadAttachmentPayload,
} from "./discussion-data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class DiscussionApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getByObject = (query: GetByObjectParams, params: RequestParams = {}) =>
    this.request<CommentDto[], ApiExceptionResponse>({
      path: `/api/discussion/get-by-object`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  getById = (query: GetByIdParams, params: RequestParams = {}) =>
    this.request<CommentDto, ApiExceptionResponse>({
      path: `/api/discussion/get-by-id`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  create = (data: CreateCommentRequest, params: RequestParams = {}) =>
    this.request<CommentDto, ApiExceptionResponse>({
      path: `/api/discussion/create`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  update = (
    { id, ...query }: UpdateParams,
    data: UpdateCommentRequest,
    params: RequestParams = {},
  ) =>
    this.request<CommentDto, ApiExceptionResponse>({
      path: `/api/discussion/update/${id}`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  delete = ({ id, ...query }: DeleteParams, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/discussion/delete/${id}`,
      method: "DELETE",
      secure: true,
      ...params,
    });
  getHistory = (
    { id, ...query }: GetHistoryParams,
    params: RequestParams = {},
  ) =>
    this.request<CommentHistoryDto[], ApiExceptionResponse>({
      path: `/api/discussion/get-history/${id}`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  uploadAttachment = (
    data: UploadAttachmentPayload,
    params: RequestParams = {},
  ) =>
    this.request<AttachmentDto, ApiExceptionResponse>({
      path: `/api/discussion/upload-attachment`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.FormData,
      format: "json",
      ...params,
    });
  deleteAttachment = (
    { id, ...query }: DeleteAttachmentParams,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/discussion/delete-attachment/${id}`,
      method: "DELETE",
      secure: true,
      ...params,
    });
  getAttachmentContent = (
    { id, ...query }: GetAttachmentContentParams,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/discussion/get-attachment-content/${id}`,
      method: "GET",
      secure: true,
      ...params,
    });
  addReaction = (data: AddReactionRequest, params: RequestParams = {}) =>
    this.request<CommentReactionDto[], ApiExceptionResponse>({
      path: `/api/discussion/add-reaction`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  removeReaction = (query: RemoveReactionParams, params: RequestParams = {}) =>
    this.request<CommentReactionDto[], ApiExceptionResponse>({
      path: `/api/discussion/remove-reaction`,
      method: "DELETE",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  searchMentionUsers = (
    query: SearchMentionUsersParams,
    params: RequestParams = {},
  ) =>
    this.request<MentionUserDto[], ApiExceptionResponse>({
      path: `/api/discussion/search-mention-users`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
}
