/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ContentType, HttpClient, RequestParams } from "oip-common";
import {
  AddReactionRequest,
  ApiExceptionResponse,
  AttachmentDto,
  CommentDto,
  CommentHistoryDto,
  CommentReactionDto,
  CreateCommentRequest,
  DeleteAttachmentParams,
  DeleteParams2,
  GetAttachmentContentParams,
  GetByIdParams,
  GetByObjectParams,
  GetHistoryParams,
  MentionUserDto,
  RemoveReactionParams,
  SearchMentionUsersParams,
  UpdateCommentRequest,
  UpdateParams2,
  UploadAttachmentPayload,
} from "./data-contracts";

@Injectable()
export class DiscussionApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Gets comments by object type and object identifier.
   *
   * @tags Discussion
   * @name getByObject
   * @summary Gets comments by object type and object identifier.
   * @request GET:/api/discussion/get-by-object
   * @secure
   * @response `200` `(CommentDto)[]` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   */
  getByObject = (query: GetByObjectParams, params: RequestParams = {}) =>
    this.request<CommentDto[], ApiExceptionResponse>({
      path: `/api/discussion/get-by-object`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Gets a comment by identifier.
   *
   * @tags Discussion
   * @name getById
   * @summary Gets a comment by identifier.
   * @request GET:/api/discussion/get-by-id
   * @secure
   * @response `200` `CommentDto` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `404` `ApiExceptionResponse` Not Found
   */
  getById = (query: GetByIdParams, params: RequestParams = {}) =>
    this.request<CommentDto, ApiExceptionResponse>({
      path: `/api/discussion/get-by-id`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Creates a new comment
   *
   * @tags Discussion
   * @name create
   * @summary Creates a new comment
   * @request POST:/api/discussion/create
   * @secure
   * @response `200` `CommentDto` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   */
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
  /**
   * @description Updates an existing comment.
   *
   * @tags Discussion
   * @name update
   * @summary Updates an existing comment.
   * @request PUT:/api/discussion/update/{id}
   * @secure
   * @response `200` `CommentDto` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   * @response `404` `ApiExceptionResponse` Not Found
   */
  update = (
    { id, ...query }: UpdateParams2,
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
  /**
   * @description Soft deletes a comment.
   *
   * @tags Discussion
   * @name delete
   * @summary Soft deletes a comment.
   * @request DELETE:/api/discussion/delete/{id}
   * @secure
   * @response `204` `void` No Content
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   * @response `404` `ApiExceptionResponse` Not Found
   */
  delete = ({ id, ...query }: DeleteParams2, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/discussion/delete/${id}`,
      method: "DELETE",
      secure: true,
      ...params,
    });
  /**
   * @description Gets edit history for a comment.
   *
   * @tags Discussion
   * @name getHistory
   * @summary Gets edit history for a comment.
   * @request GET:/api/discussion/get-history/{id}
   * @secure
   * @response `200` `(CommentHistoryDto)[]` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `404` `ApiExceptionResponse` Not Found
   */
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
  /**
   * @description Uploads an attachment for a comment.
   *
   * @tags Discussion
   * @name uploadAttachment
   * @summary Uploads an attachment for a comment.
   * @request POST:/api/discussion/upload-attachment
   * @secure
   * @response `200` `AttachmentDto` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   * @response `404` `ApiExceptionResponse` Not Found
   */
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
  /**
   * @description Deletes an attachment.
   *
   * @tags Discussion
   * @name deleteAttachment
   * @summary Deletes an attachment.
   * @request DELETE:/api/discussion/delete-attachment/{id}
   * @secure
   * @response `204` `void` No Content
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   * @response `404` `ApiExceptionResponse` Not Found
   */
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
  /**
   * @description Downloads attachment content.
   *
   * @tags Discussion
   * @name getAttachmentContent
   * @summary Downloads attachment content.
   * @request GET:/api/discussion/get-attachment-content/{id}
   * @secure
   * @response `200` `void` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `404` `ApiExceptionResponse` Not Found
   */
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
  /**
   * @description Adds or toggles a reaction for a comment.
   *
   * @tags Discussion
   * @name addReaction
   * @summary Adds or toggles a reaction for a comment.
   * @request POST:/api/discussion/add-reaction
   * @secure
   * @response `200` `(CommentReactionDto)[]` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `404` `ApiExceptionResponse` Not Found
   */
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
  /**
   * @description Removes a reaction from a comment.
   *
   * @tags Discussion
   * @name removeReaction
   * @summary Removes a reaction from a comment.
   * @request DELETE:/api/discussion/remove-reaction
   * @secure
   * @response `200` `(CommentReactionDto)[]` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `404` `ApiExceptionResponse` Not Found
   */
  removeReaction = (query: RemoveReactionParams, params: RequestParams = {}) =>
    this.request<CommentReactionDto[], ApiExceptionResponse>({
      path: `/api/discussion/remove-reaction`,
      method: "DELETE",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Searches users that can be mentioned in a comment.
   *
   * @tags Discussion
   * @name searchMentionUsers
   * @summary Searches users that can be mentioned in a comment.
   * @request GET:/api/discussion/search-mention-users
   * @secure
   * @response `200` `(MentionUserDto)[]` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   */
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
